
# Timeline Mixer ⏳

Timeline Mixer allows for blending between currently playing (or paused) timeline animation and other animations per animator.

![](Documentation~/video1.gif)


## Quick Start 🏇🏼

1) Add a ``TimelineGraph Modification Manager`` component to your scene for every timeline you want to inject into and drag the ``Timeline Director`` you want to modify in the Director field.

![](Documentation~/ModificationManager.png)

2) Add a ``Simple Animation Clip Mixer`` component to your scene and reference the ``Animator`` component you want to blend animation with.

![](Documentation~/SimpleTimelineMixer.png)

3) Add a reference to the ``Simple Animation Clip Mixer`` to your ``TimelineGraph Modification Manager`` list of mixers.

![](Documentation~/MixersList.png)

## Advanced 💡

You can create your own mixer behaviours by implementing a ``Timeline AnimationMixer`` and overriding the ``OnConnected`` and ``OnUpdate`` methods.

- ``OnConnected`` is called once when the new mixer is injected into the timeline's playable graph. Use it to add inputs to the injected ``AnimationLayerMixerPlayable`` node by calling the extension method ``mixer.AddClip`` and store the output index for blending.

- ``OnUpdate`` is called every frame with a reference to the injected mixer node and manager. Use it to update the weights on 


## Playing animation while Timeline is paused
To play animation while the ``PlayableDirector`` is paused you can call the ``RequireTimelineEvaluate()`` method on ``TimelineGraph Modification Manager`` from the ``OnUpdate`` method of your ``Timeline AnimationMixer``. This will trigger a call to ``Evaluate`` on the ``PlayableDirector`` at the end of the update.

## Contact ✒️
<b>[needle — tools for unity](https://needle.tools)</b> • 
[@NeedleTools](https://twitter.com/NeedleTools) • 
[@marcel_wiessler](https://twitter.com/marcel_wiessler) • 
[@hybridherbst](https://twitter.com/hybdridherbst)