
var Route = {

  //-- Var to enable routes
  'active' : false,

  //-- Init list coords
  'systems' : [],

  //-- Route list
  'list' : [],

  /**
   * Initialize the systems list for coordinates check
   *
   * @param {Int} Unique ID for the route
   * @param {JSON} A JSon list of points
   */

  'initRoute' : function(idRoute, route) {

    Route.active = true;

    $.each(route.points, function(key, val) {
      Route.systems[val.s] = false;
      if(val.coords != undefined) {
        val.name = val.s;
        System.create(val);
      }
    });

  },

  /**
   * Remove routes
   */

  'remove' : function() {

    $(Ed3d.catObjsRoutes).each(function(indexCat, listGrpRoutes) {
      if(listGrpRoutes != undefined)

      $(listGrpRoutes).each(function(key, indexRoute) {
        scene.remove(scene.getObjectByName( indexRoute ));
        if(scene.getObjectByName( indexRoute+'-first' ) != undefined)
          scene.remove(scene.getObjectByName( indexRoute+'-first' ));
        if(scene.getObjectByName( indexRoute+'-last' ) != undefined)
          scene.remove(scene.getObjectByName( indexRoute+'-last' ));
      });
    });


  },

  /**
   * Create a Line route
   *
   * @param {Int} Unique ID for the route
   * @param {JSON} A JSon list of points
   */

  'createRoute' : function(idRoute, route) {

    var geometryL = new THREE.Geometry();
    var nameR = '';
    var first = null;
    var last = null;
    var color = Ed3d.material.orange;
    var colorLine = Ed3d.material.line;

    var hideLast = (route.hideLast !== undefined && route.hideLast);

    //console.log(route);

    if(route.cat !== undefined && route.cat[0] != undefined && Ed3d.colors[route.cat[0]] != undefined) {
      color = new THREE.MeshBasicMaterial({
        color: Ed3d.colors[route.cat[0]]
      });
      colorLine = new THREE.MeshBasicMaterial({
        color: Ed3d.colors[route.cat[0]]
      });
    } else {
      color = Ed3d.material.orange;
    }

    $.each(route.points, function(key2, val) {

      if(Route.systems[val.s] !== false) {

        var c = Route.systems[val.s];

        if(first==null) first = [c[0], c[1], c[2]];
        last = [c[0], c[1], c[2]];

        //-- Add line point
        geometryL.vertices.push(
          new THREE.Vector3(c[0], c[1], c[2])
        );
        //Route.addPoint(c[0], c[1], c[2]);


      } else {

        console.log("Missing point: "+val.s);
      }

    });

    //-- Add lines to scene
    routes[idRoute] = new THREE.Line(geometryL, colorLine);

    //-- Add object for start & end
    if(first!==null) this.addCircle('route-'+idRoute+'-first', first, color, 7);
    if(!hideLast && last!==null)  this.addCircle('route-'+idRoute+'-last', last, color, 3);

    routes[idRoute].name = 'route-'+idRoute;

    scene.add(routes[idRoute]);


   if(route.cat !== undefined) {
     $.each(route.cat, function(keyArr, idCat) {
       if(Ed3d.catObjsRoutes[idCat] == undefined)
         Ed3d.catObjsRoutes[idCat] = [];
       Ed3d.catObjsRoutes[idCat].push(routes[idRoute].name);
     });
   }

  },

  'addCircle' : function(id, point, material, size) {

    var cursor = new THREE.Object3D;

    //-- Ring around the system
    var geometryL = new THREE.TorusGeometry( size, 0.4, 3, 15 );

    var selection = new THREE.Mesh(geometryL, material);
    selection.rotation.x = Math.PI / 2;

    cursor.add(selection);

    cursor.position.set(point[0], point[1], point[2]);

    cursor.scale.set(15,15,15);

    cursor.name = id;
    scene.add(cursor);

    //-- Add to cursor list for scale effets
    Action.cursor[cursor.id] = cursor;

  },

  'addPoint' : function(x, y, z, name) {

    /*console.log('Add point route');*/

    var cursor = new THREE.Object3D;

    //-- Ring around the system
    var geometryL = new THREE.TorusGeometry( 12, 0.4, 3, 30 );

    var selection = new THREE.Mesh(geometryL, Ed3d.material.selected);
    selection.rotation.x = Math.PI / 2;

    cursor.add(selection);

    //-- Create a cone on the selection
    var geometryCone = new THREE.CylinderGeometry(0, 5, 16, 4, 1, false);
    var cone = new THREE.Mesh(geometryCone, Ed3d.material.selected);
    cone.position.set(0, 20, 0);
    cone.rotation.x = Math.PI;
    cursor.add(cone);

    //-- Inner cone
    var geometryConeInner = new THREE.CylinderGeometry(0, 3.6, 16, 4, 1, false);
    var coneInner = new THREE.Mesh(geometryConeInner, Ed3d.material.black);
    coneInner.position.set(0, 20.2, 0);
    coneInner.rotation.x = Math.PI;
    cursor.add(coneInner);


    cursor.position.set(x, y, z);

    cursor.scale.set(30,30,30);

    scene.add(cursor);

  },

  'newRoute' : function(name) {
    var routes = localStorage.getItem("routes");
    if(routes == undefined) {
      var routes = [];
    }

    var myRoute = {
      title: "Test route",
      points: []
    }

    routes.push(myRoute);
    routes.push(myRoute);
    routes.push(myRoute);

    localStorage.setItem("routes",JSON.stringify(routes));
  },

  'addPointToRoute' : function(x,y,z,system,label) {

    idRoute = 1;

    console.log('add point');

    var routes = JSON.parse(localStorage.getItem("routes"));
    if(routes[idRoute] == undefined) return false;

    var point = {
      "coords": {"x":x, "y":y, "z":z }
    }
    if(system==undefined) system = Math.floor(Date.now() / 1000);

    if(system != undefined) point.s = system;
    if(label != undefined) point.label = label;

    routes[idRoute].points.push(point);

    localStorage.setItem("routes",JSON.stringify(routes));



    this.drawRoute(1);
  },

  'drawRoute': function(idRoute) {

    var route = JSON.parse(localStorage.getItem("routes"))[idRoute];
    if(route == undefined) return false;

    this.initRoute(0,route);
    this.createRoute(0,route);

  },



}
