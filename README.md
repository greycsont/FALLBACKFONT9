## Fffffff
A mod to add local (yes, local) font to TMP_FontAsset add it as the fallback font of game's font

## TL;DR
Add support to render non-english character

The main use of this is to render the characters in cybergrind terminal's custom music names

Here's an example:

Before:

![CGBefore](https://raw.githubusercontent.com/greycsont/Fffffff/master/docs/CGBefore.jpg)

After:

![CGAfter](https://raw.githubusercontent.com/greycsont/Fffffff/master/docs/CGAfter.jpg)

btw it seems like base game's fonts all supports russian, includes VCR OSD MONO

![UITest](https://raw.githubusercontent.com/greycsont/Fffffff/master/docs/UItest.png)

## Customization
the package includes 2 font by default: 

- fusion-pixel-font-10px-monospaced-zh_hans https://github.com/TakWolf/fusion-pixel-font used under SIL OFL version 1.1

- unifont https://www.unifoundry.com/unifont/index.html used under no I did't changed the font but it uses GPLv2+OFLv1.1 mixed license

they renamed to font1.otf and font2.otf in package

if you want to add your own (?) font into the game, name the font as font(number).otf

it will load the font from lowest number to greatest number

please don't using negative value or sth like epsilon and told to me it's a number, plz use non-negative integer

## Credit
naming inspiration from: 

DJ SHARPNEL - [Mmmmmmm](https://www.youtube.com/watch?v=DncJWznmGaA)

ref:
- OSU!: https://github.com/ppy/osu (God the UI/UX was gorgeous)

  ![osu](https://raw.githubusercontent.com/greycsont/Fffffff/master/docs/osu.png)

- Battlefield 4: Fuck you EA

  ![bf4](https://raw.githubusercontent.com/greycsont/Fffffff/master/docs/bf4.jpg)

第一个发明天才的人真是个Fallback Font

猜猜迷迭香背后的字是什么