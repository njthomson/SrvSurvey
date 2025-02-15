
# Introduction
Localized resources for SrvSurvey are spread across a number of .resx files, where the original file is named `abc.resx` and each translated file `abc.<lang>.resx`. These are initially populated with machine translations which tend to be of questionable quality.

There are some customizations in localized .resx files to make it easier to identify the state of translations. Consider these 3 resources from different languages:

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

## Translations missing and needed

To identify where translating is needed, files are generated per language. 

- `_loc-<lang>-needed.txt` lists files and resources where translations need to be updated against modified English text.
- `_loc-<lang>-missing.txt` lists "missing" resources, where English text has been machine translated, which have not been reviewed by a human.

Both files are formatted as:

```
/<relative folder>/<filename-1>.<lang>.resx
    <resource name 10>
    <resource name 11>

/<relative folder>/<filename-2>.<lang>.resx
    <resource name 20>
```

See files [_loc-de-needed.txt](_loc-de-needed.txt) or [_loc-es-needed.txt](_loc-fr-missing.txt).
 
## Pseudo language
The language `ps` is for pseudo, being non-translations of English that are excessively long, obviously different but still readable. All `*.ps.resx` files are fully generated and should be ignored.

