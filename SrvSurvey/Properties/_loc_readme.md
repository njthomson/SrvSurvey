
# Introduction

Currently SrvSurvey supports localization into the following languages:
- Deutsch (de-*)
- Español (es-*)
- Français (fr-*)
- Português (Brasil) (pt-BR)
- Русский (ru-*)
- 简体中文 (zh-Hans)

If you would like to contribute to SrvSurvey's localization, you will need to clone the repo, make changes to .resx files and then submit a pull request. To help view diff's in .resx files, building in Visual Studio will re-sort the .resx files. The same can be done by manually running [sort-resx.ps1](../sort-resx.ps1). Changes to .resx files will be visible immediately after rebuilding from local sources, then restarting SrvSurvey.

Localized resources for SrvSurvey can be found in 3 folders:
- [./SrvSurvey/Properties/](https://github.com/njthomson/SrvSurvey/tree/main/SrvSurvey/Properties)
- [./SrvSurvey/forms/](https://github.com/njthomson/SrvSurvey/tree/main/SrvSurvey/forms)
- [./SrvSurvey/plotters/](https://github.com/njthomson/SrvSurvey/tree/main/SrvSurvey/plotters)

Resources are spread across many .resx files, where the original file is named `abc.resx` and each translated file `abc.<lang>.resx`. These are initially populated with machine translations which tend to be of questionable quality. 

There are some customizations in localized .resx files to make it easier to identify the state of translations. To begin with, all localizable resources are sorted lower in the file below this line

``` XML
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <!--Localizable elements are below-->
  <data name="Alpha">
    <value>Альфа</value>
    <comment>Guardian Ruins type</comment>
    <source>Alpha</source>
  </data>
```

Resources needing attention will have a proceeding comment. Consider these 3 resources (from different languages):

``` XML
  <data name="Revolution">
    <!--Machine translated-->
    <value>Revolução</value>
    <comment>Faction state Revolution</comment>
    <source>Revolution</source>
  </data>

  <data name="Service">
    <value>Servicio</value>
    <comment>Economy type Service</comment>
    <source>Service</source>
  </data>
  
  <data name="$this.Text">
    <!--Update needed: RU-->
    <value>Сферический поиск</value>
    <source>Spherical Search</source>
  </data>
```

- `Source` is the original English source text.
- `comment` is a descriptive comment about the text, though it is not always available.
- Machine translations will be commented with `<!--Machine translated-->` which should be removed when translating by hand, 
- When the English source text changes, localized translations will be commented with `<!--Update needed: <lang>-->` which should be removed once the translation is updated.

## Want to add a new language?

Let me know which language you wish to add first, I will use machine translation to create initial files - this guarantees .resx files have the necessary comments for tracking purposes.

## Translations missing and needed

To identify where translating is needed, files are generated per language. 

- `_loc-<lang>-missing.txt` lists "missing" resources, where English text has been machine translated, which have not been reviewed by a human. Eg: 
- `_loc-<lang>-needed.txt` lists files and resources where translations need to be updated against modified English text. Machine translation is run often and it is likely these files will not be found.

Both files are formatted as:

```
/<relative folder>/<filename-1>.<lang>.resx
    <resource name 10>
    <resource name 11>

/<relative folder>/<filename-2>.<lang>.resx
    <resource name 20>
```

See files [_loc-de-needed.txt](_loc-de-needed.txt) or [_loc-fr-missing.txt](_loc-fr-missing.txt). These files will be auto updated once .resx files are submitted and should not be hand edited.
 
## Pseudo language
The language `ps` is for pseudo being non-translations of English that are excessively long, obviously different but still readable. All `*.ps.resx` files are fully generated and should be ignored.

