# Bio Criteria

Bio criteria is a format for describing conditions where genus/species/variants can be expected using .json files. This format is optimised for human readability and editability, at the expense of requiring interpretation logic in code (more about this in [mappings](#property-name-mappings) below).

Each clause takes the form of `<property name> [<condition values>]` where most conditions can be described as the property matching either:

- A numeric match between minimum and maximim values.
- A string match of one of more values.

Clauses are grouped in *"query"* arrays where every clause must be true for the group to apply, meaning, they combine with AND not OR.

This json format uses a tree structure, where clauses of parent nodes apply to their respective children. The benefit being that children do not need to redeclare the same conditions as their parents. The nodes are decorated with `genus`, `species` or `variant` names, such that when nested correctly a match will contain all 3. To prevent large files, it is assumed there is one .json file per genus.

Some clauses require some special logic, see [Special cases](#special-cases) below for more details.

To reduce file sizes and repeating clauses for colour variants, a parent node may declare an array `commonChildren`, where any child node may use `useCommonChildren: true` to use those common children instead of it's own direct children. Nodes must not contains `useCommonChildren` and a `children` array.

## Examples of clauses

| Clause | Description |
|--|--|
| `temp [146 ~ 196]` | Surface temperature must be greater than 146 and less than 196. |
| `gravity [ ~ 0.28]` | Surface gravity must be less than 0.28, with no minimum limit. |
| `pressure [0.01 ~ ]` | Surface pressure must be greater than 0.01, with no upper limit. |
| `atmosType [CarbonDioxide]` | Atmosphere type be one of: "Carbon Dioxide" |
| `volcanism [Rocky Magma,Water Magma]` | Volcanism must contain any of "Rocky Magma" or "Water Magma" |
| `volcanism [None]` | There must be no volcanism. |
| `mats $[Carbon,Sulphur]` | "Carbon" and "Sulphur" must both be present in body materials. |
| `mats ![Iron,Nickel]` | None of "Iron" or "Nickel" can be present in body materials. |
| `atmosComp [CarbonDioxide >= 100 \| SulphurDioxide >= 0.99]` | Atmospheric composition must be either 100% "Carbon Dioxide" or contain "Sulphur Dioxide" at a minimum of 0.99% |

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

# Clause types

### Range Clauses: `foo [min ~ max]` / `foo [min ~]` / `foo [~ max]`
- Matches if a numeric body property is between the min and max values.
- Both min or max clauses may be missing, signifying no upper or lower limit.

### IS Match Clause: `foo [acd]` / `foo [acd,def]`
- Matches string values, case insensitive, with some special cases called out below.
- Inner spaces must be present or missing to match journal entries.
- If clause and body properties are a singular value - they must match. Eg: `atmosType [CarbonDioxide]`
- Multiple clause values may be defined with a comma separated list. Extra outer spaces are ignored.
- If clause is a list and body property a single value - the body value must be present in the clause list. Eg: `atmosType [CarbonDioxide,SulphurDioxide]` matches if either are found.
- If clause is a list and the body property can have multiple values - matches if ANY body value is found in the clause list. Eg: `volcanism [Rocky Magma,Water Magma]` matches if either of these are found.

### ALL Match Clause: `foo &[abc]` / `foo &[abc,def]`
- Similar to IS clauses but only matches if ALL values in the clause are found, eg: `mats $[Carbon,Sulphur]` both "Carbon" and "Sulphur" must both be present in body materials.

### NOT Match Clause: `foo ![abc]` / `foo ![abc,def]`
- Similar to IS clauses but will not match if ANY clause values are found, eg: `mats ![Iron,Nickel]` will not match if either "Iron" or "Nickel" are present in body materials.

### Composition Match Clause: `atmosComp [CarbonDioxide >= 100 | SulphurDioxide >= 0.99]`
- Only valid against values from `AtmosphereComposition`.
- Sub clauses specify a name within the composition and a mimum value. Only `>=` is supported currently.
- Can define a single a sub-clause, eg: `atmosComp [Methane >= 100]` matches when "Methane" is 100%.
- Or multiple sub-clauses, matching if ANY are found. Eg: `atmosComp [Neon >= 100 | Nitrogen >= 48 | Helium >= 1]` matches if "Neon" is 100% OR "Nitrogen" is >= 48% OR "Helium" is >= 1%.

### Comment Clause: `# hello`
- Embedding `//comments` in .json files whilst useful but not strictly legal. Any clause starting with `#` will deemed a comment and ignored.

## Special cases

- **Volcanism**

    The absense of a clause for volcanism means ANY or NO form of volcanism would be a match.
    - Specifying `volcanism: None` means there must be no volcanism, matching `"Volcanism":""` from journal entries.
    - Specifying `volcanism: Some` means there must be some volcanism but it does not matter what it is.
    - It is valid to combine `None` and specific types of volcanism, eg: `volcanism: None,Rocky Magma` means there must be either no volcanism OR "Rocky Magma".
    - Spaces must be present within clause values, to match journal entries.

- **Galactic Regions**

    `region` works like a string IS match but happens to use the region ID number from their names. Hence for "Galactic Centre" which is `$Codex_RegionName_1;` use just `1`, or `7` for "Izanami", etc. See [Appending A](https://canonn.science/codex/appendices/) for a complete list of regions and their IDs.

    Eg: Tussock Cultro - Red has only been found in Odin's Hold, Izanami and Inner Orion Spur. The clause would be: `regions [4,7,18]`

- **Nebulae**

    `nebulae` works like a numeric match, specifying a maximum distance, in light years, the system must be from some nebula. It is assumed code will download/maintain a list of all known nebulae.

    Eg: For Electricae, which must be within 100ly of some nebulae the clause would be: `nebulae [ ~ 100]`

- **Guardian**

    Brain trees are required to be within 750ly or 100ly of a known bubble of Guardian ruins. It is assumed code will know the following locations. (Thank you to CMDR Alton for determining the centers of these bubbles)

    ```
    Radius      System                         Coords [x, y, z]
    750 Ly      Gamma Velorum                  [  1099.21875 , -146.6875  ,  -133.59375 ]
    750 Ly      HEN 2-333                      [  -840.65625 , -561.15625 , 13361.8125  ]

    100 Ly      Prai Hypoo OK-I b0             [ -9298.6875  , -419.40625 ,  7911.15625 ]
    100 Ly      Prua Phoe US-B d58             [ -5479.28125 , -574.84375 , 10468.96875 ]
    100 Ly      Blaa Hypai EK-C c14-1          [  1228.1875  , -694.5625  , 12341.65625 ]
    100 Ly      Eorl Auwsy SY-Z d13-3643       [  4961.1875  ,  158.09375 , 20642.65625 ]
    100 Ly      NGC 3199 Sector JH-V c2-0      [ 14602.75    , -237.90625 ,  3561.875   ]
    100 Ly      Eta Carina Sector EL-Y d1      [  8649.125   , -154.71875 ,  2686.03125 ]
    ```

    Use a clause `guardian` with Brain Trees species specifying `true` to match if within a suitable distance of a known Guardian bubble, eg: `guardian [true]`

- **Materials**

    The named material must be greater than 0.001% of composition. Eg: `mats [Cadmium]`

- **Body**

    Uses the following value mappings. Unlike other IS clauses that must match the whole string, `body` clauses match with string begins with, to allow `HMC` to match either "High metal content body" or "High metal content world" and `RockyIce` to match either "Rocky ice body" or "Rocky ice world".

    | body short-hand | Journal entry string |
    |--|--|
    | Icy| Icy body |
    | Rocky | Rocky body |
    | RockyIce | Rocky ice body or world |
    | HMC | High metal content body or world |
    | MRB | Metal rich body |

- **Star**

    Matches against the type of stars, eg: `A`, `B`, `M`, etc. Considers both the immediate parent star(s) and the "brightest" nearby star(s). This will include multiple values if orbiting barycentres.

- **ParentStar**

    Like `Star` but considers only immediate parent star(s). This will include multiple values if orbiting barycentres.

- **PrimaryStar**

    Like `Star` but considers only "primary" star for a system.

- **Atmosphere**

    Matches body property "Atmosphere" without the trailing word "atmosphere". Eg: to match against "thin neon rich atmosphere" use "thin neon rich". This property is known to have some spelling mistakes in journal entries, hence it is recommended to use `atmosType` instead.


## Property Name Mappings

- Query property name mappings, or their purpose if not a direct body property:

    | Query property name | `Scan` journal member name: |
    |--|--|
    | body | PlanetClass |
    | gravity | SurfaceGravity |
    | temp | SurfaceTemperature |
    | pressure | SurfacePressure |
    | atmosphere | Atmosphere (**Not recommended**, use `atmosType`) |
    | atmosType | AtmosphereType |
    | atmosComp | AtmosphereComposition |
    | dist | DistanceFromArrivalLS |
    | volcanism | Volcanism |
    | mats | Materials |
