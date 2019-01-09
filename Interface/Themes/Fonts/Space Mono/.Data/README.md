Space Mono by Colophon
======================

Space Mono is an original monospace display typeface family designed by Colophon Foundry for Google Design.

It supports a Latin Extended glyph set, enabling typesetting for English and other Western European languages. 

Developed explicitly for use in headline and display typography, the letterforms infuse a geometric slab core with novel over-rationalized forms. 

License
-------

Space Mono is available under the SIL Open Font License v1.1, for more details see [OFL.txt](OFL.txt)

Contributions
-------------

The project is led by Colophon, a type design foundry based in London and Los Angeles. 
To contribute ideas and feedback, see [github.com/googlefonts/spacemono](https://github.com/googlefonts/spacemono)

We may accept contributions via GitHub pull requests, but require all authors to sign the Google Contributor License. 
Please see [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

Disclaimer
----------

This is not an official Google product (experimental or otherwise,) it is just work that happens to be owned by Google.

Source Files
------------

```
└── sources
    ├── FontMenuNameDB # Contains font family names for each style naming
    ├── Roman # Two folders containing all of the information needed to build the fonts
    │   ├── Bold
    │   │   ├── GlyphOrderAndAliasDB
    │   │   ├── Space-Mono-Bold-NH.ttf                    # Non-hinted TTF
    │   │   ├── Space-Mono-Bold-source.ttf                # Hinted Source
    │   │   ├── Space-Mono-Bold.ttf                       # Final TTF
    │   │   ├── Space-Mono-Bold_QUADRATIC.vfb             # Quadratic source VFB
    │   │   ├── Space-Mono-Bold.vfb                       # Cubic (PS) source VFB
    │   │   ├── current.fpr
    │   │   ├── features
    │   │   └── fontinfo
    │   └── Regular
    │       ├── GlyphOrderAndAliasDB
    │       ├── Space-Mono-Regular-NH.ttf                 # Non-hinted TTF
    │       ├── Space-Mono-Regular-source.ttf             # Hinted Source
    │       ├── Space-Mono-Regular.ttf                    # Final TTF
    │       ├── Space-Mono-Regular_QUADRATIC.vfb          # Quadratic source VFB
    │       ├── Space-Mono-Regular.vfb                    # Cubic (PS) source VFB
    │       ├── current.fpr
    │       ├── features
    │       └── fontinfo
    └── Italics # Two folders containing all of the information needed to build the fonts
        ├── Bold Italic
        │   ├── GlyphOrderAndAliasDB
        │   ├── Space-Mono-Bold-Italic-NH.ttf             # Non-hinted TTF
        │   ├── Space-Mono-Bold-Italic-source.ttf         # Hinted Source
        │   ├── Space-Mono-Bold-Italic.ttf                # Final TTF
        │   ├── Space-Mono-Bold-Italic_QUADRATIC.vfb      # Quadratic source VFB
        │   ├── Space-Mono-Bold-ItalicC.vfb               # Cubic (PS) source VFB
        │   ├── current.fpr
        │   ├── features
        │   └── fontinfo
        └── Regular Italic
            ├── GlyphOrderAndAliasDB
            ├── Space-Mono-Regular-Italic-NH.ttf          # Non-hinted TTF
            ├── Space-Mono-Regular-Italic-source.ttf      # Hinted Source
            ├── Space-Mono-Regular-Italic.ttf             # Final TTF
            ├── Space-Mono-Regular-Italic_QUADRATIC.vfb   # Quadratic source VFB
            ├── Space-Mono-Regular-Italic.vfb             # Cubic (PS) source VFB
            ├── current.fpr
            ├── features
            └── fontinfo
```

Build Instructions
------------------

Use the [Adobe AFDKO](https://github.com/adobe-type-tools/afdko) to build each file similar to this:

    cd Sources/Roman/Regular;
    makeotf -r -f Space-Mono-Regular-source.ttf -o Space-Mono-Regular.ttf;
    ttfautohint Space-Mono-Regular.ttf Space-Mono-Regular-hinted.ttf;
    fontbakery-fix-dsig.py Space-Mono-Regular-hinted.ttf --autofix;
    mv Space-Mono-Regular-hinted.ttf.fix SpaceMono-Regular.ttf;
