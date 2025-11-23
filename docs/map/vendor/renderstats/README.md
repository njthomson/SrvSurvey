threex.rendererstats
====================

It is a three.js extension to display realtime informations about ```THREE.WebGLRenderer```.
Here is a [basic example](http://jeromeetienne.github.io/threex.rendererstats/examples/basic.html). It is widely inpired from @mrdoob [stats.js](https://github.com/mrdoob/stats.js/).
It is released under MIT license.

## How To install it

You can install it manually or with
[bower](http://bower.io/).
for the manual version, first include ```threex.rendererstats.js``` with the usual

```html
<script src='threex.rendererstats.js'></script>
```

or with
[bower](http://bower.io/) 
you type the following to install the package.

```bash
bower install -s threex.rendererstats=https://github.com/jeromeetienne/threex.rendererstats/archive/master.zip
```

then you add that in your html

```html
<script src="bower_components/threex.rendererstats/threex.rendererstats.js"></script>
```

## How To Use It

```
var rendererStats	= new THREEx.RendererStats()
```

position it on the page with css with something along this line

```
rendererStats.domElement.style.position	= 'absolute'
rendererStats.domElement.style.left	= '0px'
rendererStats.domElement.style.bottom	= '0px'
document.body.appendChild( rendererStats.domElement )
```

finally update it at every frame

```
rendererStats.update(renderer);
```