
var Grid = {

  'obj' : null,
  'size' : null,

  'textShapes' : null,
  'textGeo'    : null,

  'coordTxt'   : null,

  'minDistView' : null,

  'visible' : true,

  'fixed' : false,

  /**
   * Create 2 base grid scaled on Elite: Dangerous grid
   */

  'init' : function(size, color, minDistView) {

    this.size = size;

    this.obj = new THREE.GridHelper(1000000, size);
    this.obj.setColors(color, color);
    this.obj.minDistView = minDistView;

    scene.add(this.obj);

    this.obj.customUpdateCallback = this.addCoords;


    return this;
  },

  /**
   * Create 2 base grid scaled on Elite: Dangerous grid
   */

  'infos' : function(step, color, minDistView) {

    var size = 50000;
    if(step== undefined) step = 10000;
    this.fixed = true;

    //-- Add global grid

    var geometry = new THREE.Geometry();
    var material = new THREE.LineBasicMaterial( {
      color: 0x555555,
      transparent: true,
      opacity: 0.2,
      blending: THREE.AdditiveBlending,
      depthWrite: false
    } );

    for ( var i = - size; i <= size; i += step ) {

        geometry.vertices.push( new THREE.Vector3( - size, 0, i ) );
        geometry.vertices.push( new THREE.Vector3(   size, 0, i ) );

        geometry.vertices.push( new THREE.Vector3( i, 0, - size ) );
        geometry.vertices.push( new THREE.Vector3( i, 0,   size ) );

    }

    this.obj = new THREE.LineSegments( geometry, material );
    this.obj.position.set(0,0,-20000);

    //-- Add quadrant

    var quadrant = new THREE.Geometry();
    var material = new THREE.LineBasicMaterial( {
      color: 0x888888,
      transparent: true,
      opacity: 0.5,
      blending: THREE.AdditiveBlending,
      depthWrite: false
    } );

    quadrant.vertices.push( new THREE.Vector3( - size, 0, 20000 ) );
    quadrant.vertices.push( new THREE.Vector3(   size, 0, 20000 ) );

    quadrant.vertices.push( new THREE.Vector3( 0, 0, - size ) );
    quadrant.vertices.push( new THREE.Vector3( 0, 0,   size ) );
    var quadrantL = new THREE.LineSegments( quadrant, material );


    this.obj.add(quadrantL);


    //-- Add grid to the scene

    scene.add(this.obj);

    return this;
  },

  'addCoords' : function() {

    var textShow = '0 : 0 : 0';
    var options = {
        'font': 'helvetiker',
        'weight': 'normal',
        'style': 'normal',
        'size': this.size/20,
        'curveSegments': 10
      };

    if(this.coordGrid != null) {

      if(
        Math.abs(camera.position.y-this.obj.position.y)>this.size*10
        || Math.abs(camera.position.y-this.obj.position.y) < this.obj.minDistView
      ) {
        this.coordGrid.visible = false;
        return;
      }
      this.coordGrid.visible = true;

      var posX = Math.ceil(controls.target.x/this.size)*this.size;
      var posZ = Math.ceil(controls.target.z/this.size)*this.size;

      var textCoords = posX+' : '+this.obj.position.y+' : '+(-posZ);

      //-- If same coords as previously, return.
      if(this.coordTxt == textCoords) return;
      this.coordTxt = textCoords;

      //-- Generate a new text shape

      this.textShapes = THREE.FontUtils.generateShapes( this.coordTxt, options );
      this.textGeo.dispose();
      this.textGeo = new THREE.ShapeGeometry(this.textShapes);

      var center = this.textGeo.center();
      this.coordGrid.position.set(center.x+posX-(this.size/100), this.obj.position.y, center.z+posZ+(this.size/30));


      this.coordGrid.geometry = this.textGeo;
      this.coordGrid.geometry.needsUpdate = true;

    } else {

      this.textShapes = THREE.FontUtils.generateShapes(textShow, options);
      this.textGeo = new THREE.ShapeGeometry(this.textShapes);
      this.coordGrid = new THREE.Mesh(this.textGeo, Ed3d.material.darkblue);
      this.coordGrid.position.set(this.obj.position.x, this.obj.position.y, this.obj.position.z);
      this.coordGrid.rotation.x = -Math.PI / 2;

      scene.add(this.coordGrid);

    }

  },

  /**
   * Toggle grid view
   */
  'toggleGrid' : function() {

    this.visible = !this.visible;

    if(this.size < 10000 && isFarView) return;
    this.obj.visible = this.visible;

  },

  /**
   * Show grid
   */
  'show' : function() {

    if(!this.visible) return;

    this.obj.visible = true;

  },

  /**
   * Hide grid
   */
  'hide' : function() {

    this.obj.visible = false;

  }

}

