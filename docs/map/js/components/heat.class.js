
var Heatmap = {

  'obj' : null,


  'create' : function(values) {

    var PARTICLE_SIZE = 5000;
    var geometry  = new THREE.BufferGeometry;

    var countItem = values.length*100;
    var positions = new Float32Array( countItem * 3 );


    var texloader = new THREE.TextureLoader();
    var texSquare = texloader.load(Ed3d.basePath + "textures/lensflare/square.png");

    var particleMaterial = new THREE.PointsMaterial({
      map: texSquare,
      vertexColors: THREE.VertexColors,
      size: 800,
      fog: false,
      blending: THREE.AdditiveBlending,
      transparent: true,
      depthTest: true,
      depthWrite: false
    });

    var colors = new Float32Array( countItem * 3 );
    var color = new THREE.Color();


    var heatColor = [
      [63, 152, 180],
      [107, 196, 165],
      [165, 219, 163],
      [217, 239, 152],
      [247, 253, 177],
      [254, 245, 176],
      [253, 213, 128],
      [253, 169, 94],
      [245, 115, 69],
      [222, 73, 73]
    ];

    var count = 0;
    var grounpSize = 5;


    $.each(values, function(i, prop) {



      positions[ count ]     = prop.x;
      positions[ count + 1 ] = 0;
      positions[ count + 2 ] = -prop.z;

      var sizeColor = Math.round(prop.count/grounpSize);

      if(sizeColor>heatColor.length-1) sizeColor = heatColor.length-1;


      colors[ count ]     = heatColor[sizeColor][0]/255;
      colors[ count + 1 ] = heatColor[sizeColor][1]/255;
      colors[ count + 2 ] = heatColor[sizeColor][2]/255;

      count += 3;


      //-- TEST WITH Particle MORE !!!!
     /* var height = Math.abs(prop.y[1] - prop.y[0])/2;
      if(height>100) height = 100;
      for (var i = 1; i <height; i++) {

      positions[ count ]     = prop.x;
        positions[ count + 1 ] = i*50;
        positions[ count + 2 ] = -prop.z;

        var sizeColor = Math.round(prop.count/grounpSize);

        if(sizeColor>heatColor.length-1) sizeColor = heatColor.length-1;


        colors[ count ]     = heatColor[sizeColor][0]/255;
        colors[ count + 1 ] = heatColor[sizeColor][1]/255;
        colors[ count + 2 ] = heatColor[sizeColor][2]/255;

        count += 3;

      }*/

      //-- TEST WITH CUBE


      var height = Math.abs(prop.y[1] - prop.y[0])*2;
      if(height<100) height = 100;

      var geometry = new THREE.BoxBufferGeometry( 170, height, 170 );
      var material = new THREE.MeshLambertMaterial( {color: "rgb("+heatColor[sizeColor][0]+", "+heatColor[sizeColor][1]+", "+heatColor[sizeColor][2]+")"} );


      var cube = new THREE.Mesh( geometry, material );
      cube.position.set(prop.x, height/2, -prop.z);

      scene.add( cube );

      //-- TEST WITH CUBE


    });




    geometry.addAttribute( 'position', new THREE.BufferAttribute( positions, 3 ) );
    geometry.addAttribute( 'color', new THREE.BufferAttribute( colors, 3 ) );


    var particles = new THREE.Points(geometry, particleMaterial);

    //scene.add(particles);
  }

}
