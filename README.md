# sphirical
<img width="361" height="338" alt="image" src="https://github.com/user-attachments/assets/d3280487-fbf5-4cea-84c0-ec49a249b50c" /><br/>
sphirical is an open-source tool for unity that procedurally generates planets which is really useful for replayability

## how to use
you can test it from [its itch.io page](https://cof99.itch.io/procedural-planet-generator)<br/> and if you liked it you can just take the assets folder and the material folder and attache the planet generator script to an object with a mesh filter + mesh renderer

## how it works
it first makes a cube sphere which looks like this<br/>
<img width="375" height="200" alt="gif of a cube turning into a sphere" src="https://user-cdn.hackclub-assets.com/019f8f5d-3290-7bec-af8f-3febfdabb1cf/giphy.gif"/> <img width="200" height="200" alt="image" src="https://github.com/user-attachments/assets/55137165-fea2-47b6-8735-79ef0d412bc7" />
<br/>
then it uses 3d [perlin noise](https://en.wikipedia.org/wiki/Perlin_noise) with octaves to move the vertices towards or away from the sphere's canter<br/>
then it colors the planet based on the height and the steepness using two gradients one for steep terrain and the other for flat terrain and blends between the two using a [curve](https://docs.unity3d.com/6000.5/Documentation/Manual/animeditor-AnimationCurves.html)
