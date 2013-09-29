[VKPlayer]
Measure=Plugin
Plugin=VKPlayer.dll

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

[MeterTitle]
Meter=STRING
MeasureName=Title
X=50
Y=15
FontFace=Calibri
FontSize=10
FontColor=250,250,250,255
StringAlign=LEFT
AntiAlias=1

[MeterArtist]
Meter=STRING
MeasureName=Artist
X=50
Y=0
FontFace=Calibri
FontSize=10
FontColor=250,250,250,255
AntiAlias=1



;-------------------------
; BUTTONS
[Album]
Meter=Image
ImageName=Player\MusicAlbum.W.png
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "OnlineInfo"][!Update]
X=0
Y=0

[PlayPause]
Meter=Image
ImageName=Player\MusicPlay.W.png
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "PlayPause"][!Update]
W=12
H=12
X=70
Y=32

[Stop]
Meter=Image
ImageName=Player\MusicStop.W.png
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "Stop"][!Update]
W=12
H=12
X=90
Y=32

[Next]
Meter=Image
ImageName=Player\MusicNext.W.png
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "Next"][!Update]
W=12
H=12
X=110
Y=32

[Prev]
Meter=Image
ImageName=Player\MusicPrevious.W.png
LeftMouseUpAction=[!CommandMeasure "VKPlayer" "Previous"][!Update]
W=12
H=12
X=50
Y=32

[Repeat]
Meter=Image
ImageName=Player\MusicRepeat.W.png
ImageAlpha=128
W=12
H=12
X=130
Y=32
LeftMouseUpAction=[][!Update]
DynamicVariables=1

[Shuffle]
Meter=Image
ImageName=Player\MusicShuffle.W.png
ImageAlpha=128
W=12
H=12
X=150
Y=32
DynamicVariables=1

;[CountDown]
;Measure=Calc
;Formula=(CountDown-1+([Duration]+1))%([Duration]+1)
;MinValue=0
;MaxValue=[Duration]


[Progress]
MeasureName=Duration
Meter=BAR
Flip=1
BarColor=150,150,150,255
SolidColor=185,250,160,255
BarOrientation=Horizontal
x=0
y=45
W=200
H=10