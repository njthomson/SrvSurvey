
var Action = {

  'cursorSel' : null,
  'cursorHover' : null,
  'cursorScale' : 1,


  'cursor' : {
    'selection' : null,
    'hover' : null,
  },

  'mouseVector' : null,
  'raycaster' : null,
  'oldSel' : null,
  'objHover' : null,
  'mouseUpDownTimer' : null,
  'mouseHoverTimer' : null,
  'animPosition' : null,

  'prevScale' : null,

  'pointCastRadius' : 2,

  'pointsHighlight' : [],

  /**
   * Init Raycaster for events on Systems
   */

  'init' : function() {

    this.mouseVector = new THREE.Vector3();
    this.raycaster = new THREE.Raycaster();

    var obj = this;

    container.addEventListener('mousedown', function(e){obj.onMouseDown(e,obj);}, false);
    container.addEventListener('mouseup', function(e){obj.onMouseUp(e,obj);}, false);
    container.addEventListener('mousemove', function(e){obj.onMouseHover(e,obj);}, false);

    container.addEventListener('mousewheel', this.stopWinScroll, false );
    container.addEventListener('DOMMouseScroll', this.stopWinScroll, false ); // FF


    if(Ed3d.showNameNear) {
      console.log('Launch EXPERIMENTAL func');
      window.setInterval(function(){
        obj.highlightAroundCamera(obj);
      }, 1000);
    }
  },

  /**
   * Stop window scroll when mouse on scene
   */

  'stopWinScroll' : function (event) {
    event.preventDefault();
    event.stopPropagation();
  },

  /**
   * Update point click radius: increase radius with distance
   */

  'updatePointClickRadius' : function (radius) {
    radius = Math.round(radius);
    if(radius<2) radius = 2;
    if(this.pointCastRadius == radius) return;
    this.pointCastRadius = radius;
  },

  /**
   * Update particle size on zoom in/out
   */

  'sizeOnScroll' : function (scale) {

    if(System.particle == undefined || scale<=0) return;

    var minScale = Ed3d.effectScaleSystem[0];
    var maxScale = Ed3d.effectScaleSystem[1];
    var newScale = scale*20;

    if(this.prevScale == newScale) return;
    this.prevScale = newScale;


    if(newScale>maxScale) newScale = maxScale;
    if(newScale<minScale) newScale = minScale;

    System.particle.material.size = newScale;
    System.scaleSize = newScale;

  },

  /**
   * Highlight selection around camera target (EXPERIMENTAL)
   */

  'highlightAroundCamera' : function (obj) {

    if(isFarView == true) return;

    var newSel = [];
    var limit = 50;
    var count = 0;

    var raycaster = new THREE.Raycaster(camera.position, camera.position);
    raycaster.params.Points.threshold = 100;

    var intersects = raycaster.intersectObjects(scene.children);
    if (intersects.length > 0) {

      //-- Highlight new selection

      for( var i = 0; i < intersects.length; i++ ) {

        if(count>limit) return;

        var intersection = intersects[ i ];
        if(intersection.object.clickable) {

          var indexPoint = intersection.index;
          var selPoint = intersection.object.geometry.vertices[indexPoint];

          if(selPoint.visible) {
            var textAdd = selPoint.name;
            var textId = 'highlight_'+indexPoint
            if(obj.pointsHighlight.indexOf(textId) == -1) {

              HUD.addText(textId,  textAdd, 0, 4, 0, 1, selPoint, true);

              obj.pointsHighlight.push(textId);
            }
            newSel[textId] = textId;
          }

          count++;

        }
      }

    }

    //-- Remove old selection

    $.each(obj.pointsHighlight, function(key, item) {
      if(newSel[item] == undefined) {
        var object = Ed3d.textSel[item];
        if(object != undefined) {
          scene.remove(object);
          Ed3d.textSel.splice(item, 1);
          obj.pointsHighlight.splice(key, 1);
        }
      }
    });

  },


  /**
   * Mouse Hover
   */

  'onMouseHover' : function (e, obj) {

    e.preventDefault();

    var position = $('#ed3dmap').offset();
    var scrollPos = $(window).scrollTop();
    position.top -= scrollPos;

    obj.mouseVector = new THREE.Vector3(
      ( ( e.clientX - position.left ) / renderer.domElement.width ) * 2 - 1,
      - ( ( e.clientY - position.top ) / renderer.domElement.height ) * 2 + 1,
      1);

    obj.mouseVector.unproject(camera);
    obj.raycaster = new THREE.Raycaster(camera.position, obj.mouseVector.sub(camera.position).normalize());
    obj.raycaster.params.Points.threshold = obj.pointCastRadius;

    // create an array containing all objects in the scene with which the ray intersects
    var intersects = obj.raycaster.intersectObjects(scene.children);
    if (intersects.length > 0) {

      for( var i = 0; i < intersects.length; i++ ) {
        var intersection = intersects[ i ];
        if(intersection.object.clickable) {

          var indexPoint = intersection.index;
          var selPoint = intersection.object.geometry.vertices[indexPoint];

          if(selPoint.visible) {
            Action.hoverOnObj(indexPoint);
            return;
          }
        }
      }



    } else {
      Action.outOnObj();
    }

  },

  'hoverOnObj' : function (indexPoint) {

    if(this.objHover == indexPoint) return;
    this.outOnObj();

    this.objHover = indexPoint;

    var sel = System.particleGeo.vertices[indexPoint];
    this.addCursorOnHover(sel);

  },

  'outOnObj' : function () {

    if(this.objHover === null || System.particleGeo.vertices[this.objHover] == undefined)
      return;

    this.objHover = null;
    this.cursor.hover.visible = false;

  },

  /**
   * On system click
   */

  'onMouseDown' : function (e, obj) {

    obj.mouseUpDownTimer = Date.now();

  },


  /**
   * On system click
   */

  'onMouseUp' : function (e, obj) {

    e.preventDefault();

    //-- If long clic down, don't do anything

    var difference = (Date.now()-obj.mouseUpDownTimer)/1000;
    if (difference > 0.2) {
      obj.mouseUpDownTimer = null;
      return;
    }
    obj.mouseUpDownTimer = null;

    //-- Raycast object

    var position = $('#ed3dmap').offset();
    var scrollPos = $(window).scrollTop();
    position.top -= scrollPos;

    obj.mouseVector = new THREE.Vector3(
      ( ( e.clientX - position.left ) / renderer.domElement.width ) * 2 - 1,
      - ( ( e.clientY - position.top ) / renderer.domElement.height ) * 2 + 1,
      1);


    obj.mouseVector.unproject(camera);
    obj.raycaster = new THREE.Raycaster(camera.position, obj.mouseVector.sub(camera.position).normalize());
    obj.raycaster.params.Points.threshold = obj.pointCastRadius;


    // create an array containing all objects in the scene with which the ray intersects
    var intersects = obj.raycaster.intersectObjects(scene.children);
    if (intersects.length > 0) {

      for( var i = 0; i < intersects.length; i++ ) {
        var intersection = intersects[ i ];
        if(intersection.object.clickable) {

          var indexPoint = intersection.index;
          var selPoint = intersection.object.geometry.vertices[indexPoint];

          if(selPoint.visible) {
            $('#hud #infos').html(
              "<h2>"+selPoint.name+"</h2>"
            );

            var isMove = obj.moveToObj(indexPoint, selPoint);

            var opt = [ selPoint.name ];

            var optInfos = (selPoint.infos != undefined) ? selPoint.infos : null;
            var optUrl   = (selPoint.url != undefined) ? selPoint.url : null;

            $(document).trigger( "systemClick", [ selPoint.name, optInfos, optUrl ] );

            if(isMove) return;
          }

        }

        if(intersection.object.showCoord) {

          $('#debug').html(Math.round(intersection.point.x)+' , '+Math.round(-intersection.point.z));

          //Route.addPointToRoute(Math.round(intersection.point.x),0,Math.round(-intersection.point.z));

        }
      }

    }

    obj.disableSelection();

  },

  /**
   * Move to the next visible system
   *
   * @param {int} indexPoint
   */

  'moveNextPrev' : function (indexPoint, increment) {

    var find = false;
    while(!find) {

      //-- If next|previous is undefined, loop to the first|last
      if (indexPoint < 0) indexPoint = System.particleGeo.vertices.length-1;
      else if (System.particleGeo.vertices[indexPoint] == undefined) indexPoint = 0;

      if(System.particleGeo.vertices[indexPoint].visible == true) {
        find = true;
      } else {
        indexPoint += increment
      }
    }


    //-- Move to
    var selPoint = System.particleGeo.vertices[indexPoint];
    this.moveToObj(indexPoint, selPoint);

  },

  /**
   * Disable current selection
   */

  'disableSelection' : function () {

    if(this.cursor.selection == null) return;

    this.oldSel = null;
    this.cursor.selection.visible = false;

    $('#hud #infos').html('');

  },

  /**
   * Move to inital position without animation
   */
  'moveInitalPositionNoAnim' : function (timer) {

    var cam = [Ed3d.playerPos[0], Ed3d.playerPos[1]+500, -Ed3d.playerPos[2]+500];
    if(Ed3d.cameraPos != null) {
      cam = [Ed3d.cameraPos[0], Ed3d.cameraPos[1], -Ed3d.cameraPos[2]];
    }

    var moveTo = {
      x: cam[0], y: cam[1], z: cam[2],
      mx: Ed3d.playerPos[0], my: Ed3d.playerPos[1] ,   mz: -Ed3d.playerPos[2]
    };
    camera.position.set(moveTo.x, moveTo.y, moveTo.z);
    controls.target.set(moveTo.mx, moveTo.my, moveTo.mz);

  },

  /**
   * Move to inital position
   */
  'moveInitalPosition' : function (timer) {

    if(timer == undefined) timer = 800;

    this.disableSelection();

    //-- Move camera to initial position

    var moveFrom = {
      x: camera.position.x, y: camera.position.y , z: camera.position.z,
      mx: controls.target.x, my: controls.target.y , mz: controls.target.z
    };

    //-- Move to player position if defined, else move to Sol
    var cam = [Ed3d.playerPos[0], Ed3d.playerPos[1]+500, -Ed3d.playerPos[2]+500]
    if(Ed3d.cameraPos != null) {
      cam = [Ed3d.cameraPos[0], Ed3d.cameraPos[1], -Ed3d.cameraPos[2]];
    }

    var moveCoords = {
      x:  cam[0], y:  cam[1], z: cam[2],
      mx: Ed3d.playerPos[0], my: Ed3d.playerPos[1] ,   mz: -Ed3d.playerPos[2]
    };

    controls.enabled = false;

    //-- Remove previous anim
    if(Ed3d.tween != null) {
      TWEEN.removeAll();
    }

    //-- Launch anim
    Ed3d.tween = new TWEEN.Tween(moveFrom, {override:true}).to(moveCoords, timer)
      .start()
      .onUpdate(function () {
        camera.position.set(moveFrom.x, moveFrom.y, moveFrom.z);
        controls.target.set(moveFrom.mx, moveFrom.my, moveFrom.mz);
      })
      .onComplete(function () {
        controls.enabled = true;
        controls.update();
      });

  },


  /**
   * Move camera to a system
   *
   * @param {int} index - The system index
   * @param {object} obj - System datas
   */

  'moveToObj' : function (index, obj) {

    if (this.oldSel !== null && this.oldSel == index)  return false;

    controls.enabled = false;

    HUD.setInfoPanel(index, obj);

    if(obj.infos != undefined) HUD.openHudDetails();


    this.oldSel = index;
    var goX = obj.x;
    var goY = obj.y;
    var goZ = obj.z;

    //-- If in far view reset to classic view
    disableFarView(25, false);

    //-- Move grid to object
    this.moveGridTo(goX, goY, goZ);

    //-- Move camera to target (Smooth move using Tween)

    var moveFrom = {
      x: camera.position.x, y: camera.position.y , z: camera.position.z,
      mx: controls.target.x, my: controls.target.y , mz: controls.target.z
    };
    var moveCoords = {
      x: goX, y: goY + 15, z: goZ + 15,
      mx: goX, my: goY , mz: goZ
    };

    Ed3d.tween = new TWEEN.Tween(moveFrom, {override:true}).to(moveCoords, 800)
      .start()
      .onUpdate(function () {
        camera.position.set(moveFrom.x, moveFrom.y, moveFrom.z);
        controls.target.set(moveFrom.mx, moveFrom.my, moveFrom.mz);
      })
      .onComplete(function () {
        controls.update();
      });

    //-- 3D Cursor on selected object

    obj.material = Ed3d.material.selected;

    this.addCursorOnSelect(goX, goY, goZ);

    //-- Add text
    var textAdd = obj.name;
    var textAddC = Math.round(goX) + ', ' + Math.round(goY) + ', ' + Math.round(-goZ);

    HUD.addText('system',  textAdd, 8, 20, 0, 6, this.cursor.selection);
    HUD.addText('coords',  textAddC, 8, 15, 0, 3, this.cursor.selection);

    controls.enabled = true;

    return true;

  },

  /**
   * Create a cursor on selected system
   *
   * @param {number} x
   * @param {number} y
   * @param {number} z
   */

  'addCursorOnSelect' : function (x, y, z) {

    if(this.cursor.selection == null) {
      this.cursor.selection = new THREE.Object3D();

      //-- Ring around the system
      var geometryL = new THREE.TorusGeometry( 8, 0.4, 3, 20 );

      var selection = new THREE.Mesh(geometryL, Ed3d.material.selected);
      //selection.position.set(x, y, z);
      selection.rotation.x = Math.PI / 2;

      this.cursor.selection.add(selection);

      //-- Create a cone on the selection
      var geometryCone = new THREE.CylinderGeometry(0, 5, 16, 4, 1, false);
      var cone = new THREE.Mesh(geometryCone, Ed3d.material.selected);
      cone.position.set(0, 20, 0);
      cone.rotation.x = Math.PI;
      this.cursor.selection.add(cone);

      //-- Inner cone
      var geometryConeInner = new THREE.CylinderGeometry(0, 3.6, 16, 4, 1, false);
      var coneInner = new THREE.Mesh(geometryConeInner, Ed3d.material.black);
      coneInner.position.set(0, 20.2, 0);
      coneInner.rotation.x = Math.PI;
      this.cursor.selection.add(coneInner);



      scene.add(this.cursor.selection);
    }

    this.cursor.selection.visible = true;
    this.cursor.selection.position.set(x, y, z);
    this.cursor.hover.scale.set(this.cursorScale, this.cursorScale, this.cursorScale);

  },

  /**
   * Create a cursor on hover
   *
   * @param {number} x
   * @param {number} y
   * @param {number} z
   */

  'addCursorOnHover' : function (obj) {

    if(this.cursor.hover == null) {
      this.cursor.hover = new THREE.Object3D();

      //-- Ring around the system
      var geometryL = new THREE.TorusGeometry( 6, 0.4, 3, 20 );

      var selection = new THREE.Mesh(geometryL, Ed3d.material.grey);
      selection.rotation.x = Math.PI / 2;

      this.cursor.hover.add(selection);

      scene.add(this.cursor.hover);
    }

    this.cursor.hover.position.set(obj.x, obj.y, obj.z);
    this.cursor.hover.visible = true;
    this.cursor.hover.scale.set(this.cursorScale, this.cursorScale, this.cursorScale);

    //-- Add text

    var textAdd = obj.name;
    HUD.addText('system_hover',  textAdd, 0, 4, 0, 3, this.cursor.hover);

  },

  /**
   * Update cursor size with camera distance
   *
   * @param {number} distance
   */

  'updateCursorSize' : function (scale) {

    var obj = this;

    $.each(this.cursor, function(key, cur) {
      if(cur != null) {
        cur.scale.set(scale, scale, scale);
      }
    });

    this.cursorScale = scale

  },

  /**
   * Move grid to selection
   *
   * @param {number} goX
   * @param {number} goY
   * @param {number} goZ
   */

  'moveGridTo' : function (goX, goY, goZ) {
    var posX = Math.floor(goX/1000)*1000;
    var posY = Math.floor(goY);
    var posZ = Math.floor(goZ/1000)*1000;

    if(!Ed3d.grid1H.fixed)  Ed3d.grid1H.obj.position.set(posX, posY, posZ);
    if(!Ed3d.grid1K.fixed)  Ed3d.grid1K.obj.position.set(posX, posY, posZ);
    if(!Ed3d.grid1XL.fixed) Ed3d.grid1XL.obj.position.set(posX, posY, posZ);

  }



}