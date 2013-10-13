[Variables]
Repeat=False
Shuffle=False

[VKPlayer]
Measure=Plugin
Plugin=VKPlayer.dll
Type=Player

[State]
Measure=Plugin
Plugin=VKPlayer.dll
Type=State

[Artist]
Measure=Plugin
Plugin=VKPlayer.dll
Type=Artist

[Title]
Measure=Plugin
Plugin=VKPlayer.dll
Type=Title

[Duration]
Measure=Plugin
Plugin=VKPlayer.dll
Type=Duration

[Time]
Measure=Plugin
Plugin=VKPlayer.dll
Type=Time
MinValue=0
MaxValue=[Duration]
DynamicVariables=1

[Repeat]
Measure=Plugin
Plugin=VKPlayer.dll
Type=Repeat

[Shuffle]
Measure=Plugin
Plugin=VKPlayer.dll
Type=Shuffle

[MeterArtist]
Meter=STRING
MeasureName=Artist
X=10
Y=0
FontFace=Proxima
FontSize=10
FontColor=250,250,250,255
AntiAlias=1

[MeterTitle]
Meter=STRING
MeasureName=Title
X=10
Y=15
FontFace=Proxima
FontSize=10
FontColor=250,250,250,255
AntiAlias=1


[MeterPrev]
Meter=Image
MeterStyle=Button
ImageName=Player\MusicPrevious.W.png
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "Previous"][!Update]
X=12

[MeterPlayPause]
Meter=Image
MeterStyle=Button
MeasureName=State
ImageName=Player\Music.%1.W.png
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "PlayPause"][!Update]

[MeterNext]
Meter=Image
MeterStyle=Button
ImageName=Player\MusicNext.W.png
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "Next"][!Update]

[MeterRepeat]
Meter=Image
MeterStyle=Button
ImageName=Player\MusicRepeat.W.png
ImageAlpha=[Repeat]
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "Repeat"][!Update]
DynamicVariables=1

[MeterShuffle]
Meter=Image
MeterStyle=Button
ImageName=Player\MusicShuffle.W.png
ImageAlpha=[Shuffle]
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "Shuffle"][!Update]
DynamicVariables=1

[Button]
W=12
H=12
X=20r
Y=35

[Progress]
MeasureName=Time
Meter=BAR
Flip=0
BarColor=185,250,160,255
SolidColor=150,150,150,255
BarOrientation=Horizontal
x=12
y=50
W=180
H=10