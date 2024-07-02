# Bio Criteria

Bio criteria is a format for describing conditions where genus/species/variants can be expected using .json files. This format is optimised for human readability and editability, at the expense of requiring interpretation logic in code (more about this in [mappings](#property-name-mappings) below).

Each clause takes the form of `<property name> [<condition values>]` where most conditions can be described as the property matching either:

- A numeric match between minimum and maximim values.
- A string match of one of more values.

Clauses are grouped in *"query"* arrays where every clause must be true for the group to apply, meaning, combine with AND not OR.

This json format uses a tree structure, where clauses of parent nodes apply to their respective children. The benefit being that children do not need to redeclare the same conditions as their parents. The nodes are decorated with `genus`, `species` or `variant` names, such that when nested correctly a match will contain all 3. To prevent large files, it is assumed there is one .json file per genus.

Some clauses require some special logic, see [Special cases](#special-cases) below for more details.

## Examples of basic clauses

| Clause | Description |
|--|--|
| `temp [146 ~ 196]` | Surface temperature must be greater than 146 and less than 196. |
| `gravity [ ~ 0.28]` | Surface gravity must be less than 0.28, with no minimum limit. |
| `pressure [0.01 ~ ]` | Surface pressure must be greater than 0.01, with no upper limit. |
| `atmosphere [Thin Ammonia]` | Atmosphere must be "Thin Ammonia" |
| `atmosType [CarbonDioxide,SulphurDioxide,Water]` | Atmosphere type must be one of: "Carbon Dioxide", "Sulphur Dioxide", or "Water" |
| `volcanism [None]` | There must be no volcanism. |

## Example tree of clauses

In this example:
- All genus Concha must be on either HMC or Rocky bodies, which has 2 child nodes.
- Species Aureolas requires an Ammonia atmosphere type with a minimum and maximum value for temperature, and only a max value for gravity.
- Species Labiata requires a Carbon Dioxide atmosphere with different values for temperative and gravity, plus no volcanism.
- Both species would have 10 child nodes for matching the different star types needed for each colour variant.
- Excess white space in query strings is ignored, allowing for clauses to be arranged such that they align vertically for easier reading.

``` json
  {
    "genus": "Conchas",
    "query": [
      "body [HMC,Rocky]"
    ],
    "children": [

      {
        "species": "Aureolas",
        "query": [
            "atmosType [Ammonia]",
            "  gravity [ ~ 0.27]",
            "     temp [152 ~ 177]"
        ],
        "children": [
          { "variant": "Indigo", "query": [ " star [B]" ] },
          { "variant": "etc", "query": [ "    star [etc]" ] }
          { "variant": "Emerald", "query": [ "star [N]" ] },
        ]
      },

      {
        "species": "Labiata",
        "query": [
            "atmosType [CarbonDioxide]",
            "  gravity [ ~ 0.26]",
            "     temp [150 ~ 199]",
            "volcanism [None]"
          ],
        "children": [
          { "variant": "Indigo", "query": [ " star [B]" ] },
          { "variant": "etc", "query": [ "    star [etc]" ] }
          { "variant": "Emerald", "query": [ "star [N]" ] },
        ]
      }

    ]
  }
```

## Special cases

- **Volcanism**

    The absense of a clause for volcanism means any form of volcanism or NO volcanism would be a match.
    - Specifying `"volcanism: None"` means there must be no volcanism, being `"Volcanism":""` in journal entries.
    - Specifying `"volcanism: Some"` means there must be some volcanism but it does not matter what it is.
        > TODO: Is this useful? Or should we explicitly declare all the actual types?

- **Galactic Regions**

    `region` works like a string match but happens to use the region ID number from their names. Hence for "Galactic Centre" which is `$Codex_RegionName_1;` use just `1`, or `7` for "Izanami", etc. See [Appending A](https://canonn.science/codex/appendices/) for a complete list of regions and their IDs.

    Eg: Tussock Cultro - Red has only been found in Odin's Hold, Izanami and Inner Orion Spur. The clause would be: `"regions [4,7,18]"`


- **Nebulae**

    `nebulae` works like a numberic match, specifying a maximum distance, in light years, the system must be from some nebula. It is assumed code will download/maintain a list of all known nebulae.

    Eg: For Electricae, which must be within 100ly of some nebulae the clause would be: `"nebulae [ ~ 100]"`


- **Brain Trees**

    Brain trees are required to be within 750ly or 100ly of a known bubble of Guardian ruins. It is assumed code will know the following locations. (Thank you to CMDR Alton for determining the centers of these bubbles)

    ```
    Radius      System                         Coords [x, y, z]
    750 Ly      Gamma Velorum                  [  1099.21875 ,  -146.6875 , -133.59375  ]
    750 Ly      HEN 2-333                      [  -840.65625 , -561.15625 , 13361.8125  ]

    100 Ly      Prai Hypoo OK-I b0             [  -9298.6875 , -419.40625 , 7911.15625  ]
    100 Ly      Prua Phoe US-B d58             [ -5479.28125 , -574.84375 , 10468.96875 ]
    100 Ly      Blaa Hypai EK-C c14-1          [   1228.1875 ,  -694.5625 , 12341.65625 ]
    100 Ly      Eorl Auwsy SY-Z d13-3643       [   4961.1875 ,  158.09375 , 20642.65625 ]
    100 Ly      NGC 3199 Sector JH-V c2-0      [    14602.75 , -237.90625 , 3561.875    ]
    100 Ly      Eta Carina Sector EL-Y d1      [    8649.125 , -154.71875 , 2686.03125  ]
    ```

    We will include a clause for Brain Tree species but no actual clause criteria are needed, eg: `"guardian []"`

- **AtmosphericComposition and Materials**

    The named gas must be greater than ... ?

    > TODO: Greater than some fixed amout, eg: `"atmosComp [Argon]"` assumes greater than 1%. But is this correct and what is that fixed amount?

    > TODO: Or greater than some declared number. This breaks from the standard pattern above by embedding a sub-clause and numeric value with the string, eg: `"atmosComp [Argon > 0.05, Neon < 1]"`


- **Materials**

    The named material must be greater than 0.001% of composition. Eg: `"mats [Cadmium]"`
    > TODO: confirm this.


- **Parent stars or brightest star**

    > TODO: TBD ...


- **Comments**

    Embedding `//comments` is not strictly legal in .json files. Any clause starting with `#` will deemed a comment and ignored by code.


## Property Name Mappings

- Query property name mappings:

    | Query property name | `Scan` journal member name: |
    |--|--|
    | body | PlanetClass |
    | gravity | SurfaceGravity |
    | temp | SurfaceTemperature |
    | pressure | SurfacePressure |
    | atmosphere | Atmosphere |
    | atmosType | AtmosphereType |
    | atmosComp | AtmosphereComposition |
    | dist | DistanceFromArrivalLS |
    | volcanism | Volcanism |
    | materials | Materials |

- Body/PlanetClass mappings:

    | body short-hand | Journal entry string |
    |--|--|
    | Icy| Icy body |
    | Rocky | Rocky body |
    | RockyIce | Rocky ice body |
    | HMC | High metal content body |

