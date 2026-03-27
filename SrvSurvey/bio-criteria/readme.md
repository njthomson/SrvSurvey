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
| `gravity [ ~ 0.28]` | Surface gravity must be less than 0.28, with no minimum limit.|
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
    
    Alternatively, species can use the galactic arms corresponding to specific sets of regions:
    - Orion-CygnusArm: { 7, 8, 9, 16, 17, 18, 35 }
    - OuterArm": { 5, 6, 13, 14, 27, 29, 31, 41, 37 }
    - Scutum-CentaurusArm": { 9, 10, 11, 12, 24, 25, 26, 42, 28 }
    - PerseusArm": { 15, 30, 32, 33, 34, 36, 38, 39 }
    - Sagittarius-CarinaArm": { 9, 18, 19, 20, 21, 22, 23, 40 }
    - CentreLeft": { 1, 4 }
    - CentreTop": { 1, 3, 7 }
    - CentreRight": {1, 2 }
    - AmphoraBatch": { 10, 19, 20, 21, 22 }
    - AnemoneBatch": { 7, 8, 9, 13, 14, 15, 16, 17, 18, 27, 31 }
    - BrainTreeBatch": { 2, 9, 10, 17, 18, 35 }
    - TubersBatch": { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 18, 19 }

    Eg: regions `[Sagittarius-CarinaArm,CentreLeft]` contains the regions associated with both Sagittarius-CarinaArm and CentreLeft. `![Sagittarius-CarinaArm,CentreLeft]` would contain all regions except those in Sagittarius-CarinaArm or CentreLeft.

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

## Known body absolute limits

- Certain bodies have known limits that do not have to be specified if very close.
- Landable pressure caps at 0.1atm, and minimum approx 0.00098
- Ammonia icy is 0.026g-0.23g
- Ammonia rocky ice is 0.03g-0.33g
- Ammonia rocky is 0.04g-0.36g
- Ammonia HMC is 0.05g-0.376g
- Ammonia-rich is rocky bodies with 0.75g-0.79g (no known bio)
- Ammonia icy has a max pressure of 0.0157atm
- Ammonia rocky ice has a max pressure of 0.0147atm
- Ammonia rocky has a max pressure of 0.0134atm
- Ammonia HMC has a max pressure of 0.0133atm
- Ammonia ice has a surface temperature of 155K-176.58K
- Ammonia rocky ice has a surface temperature of 157.4K-176.63K
- Ammonia rocky has a surface temperature of 152K-176.77K
- Ammonia HMC has a surface temperature of 152K-176.67K
 
- Argon icy is 0.03g-0.54g.
- Argon rocky ice is 0.06g-0.58g.
- Argon rocky is 0.05g-1.26g.
- Argon HMC is 0.14g-1.48g.
- Argon-rich icy is 0.21g-0.47g.
- Argon-rich rocky ice is 0.24g-0.49g.
- Argon-rich rocky is 0.27g-1.13g.
- Argon-rich HMC is 0.30g-1.47g.
- All argon have a pressure of 0.001atm-0.1atm
- Argon-rich icy has a pressure of 0.011atm-0.1atm.
- Argon-rich rocky ice has a pressure of 0.007atm-0.1atm.
- Argon-rich rocky has a pressure of 0.001atm-0.1atm.
- Argon-rich HMC has a pressure of 0.001atm-0.1atm.
- Argon icy has a surface temperature of 50K-317K.
- Argon rocky ice has a surface temperature of 50K-310K.
- Argon rocky has a surface temperature of 50K-236K.
- Argon HMC has a surface temperature of 50K-339K.
- Argon-rich icy has a surface temperature of 60K-238K.
- Argon-rich rocky ice has a surface temperature of 59K-233K.
- Argon-rich rocky has a surface temperature of 50K-309K.
- Argon-rich HMC has a surface temperature of 50K-333K.

- Carbon Dioxide icy is 0.25g-0.34g.
- Carbon Dioxide rocky ice is 0.30g-0.51g
- Carbon Dioxide rocky is 0.04g-1.34g. Typical bio is ???-0.238g.
- Carbon Dioxide HMC is 0.044g-1.45g.
- Carbon Dioxide Metal-rich has insufficient data.
- Carbon Dioxide-rich icy is 0.25g-0.56g.
- Carbon Dioxide-rich rocky ice is 0.35g-0.58g.
- Carbon Dioxide-rich rocky is 0.33g-1.34g.
- Carbon Dioxide-rich HMC is 0.35g-1.45g.
- Carbon Dioxide rocky has a pressure of 0.001atm-0.1atm
- Carbon Dioxide HMC has a pressure of 0.001atm-0.1atm
- Carbon Dioxide-rich (rocky) ice has a pressure of 0.001atm-0.1atm
- Carbon Dioxide-rich rocky has a pressure of 0.0025atm-0.1atm
- Carbon Dioxide-rich HMC has a pressure of 0.0017atm-0.1atm
- Carbon Dioxide icy has a surface temperature of 153K-200.5K.
- Carbon Dioxide rocky ice has a surface temperature of 158K-226K.
- Carbon Dioxide rocky has a surface temperature of 145K-500K.
- Carbon Dioxide HMC has a surface temperature of 143K-500K.
- Carbon Dioxide-rich icy has a surface temperature of 151K-266K. Typical bio is 170-180K.
- Carbon Dioxide-rich rocky ice has a surface temperature of 151K-299K.
- Carbon Dioxide-rich rocky has a surface temperature of 159K-322K. Typical bio is ???-246K.
- Carbon Dioxide-rich HMC has a surface temperature of 156K-328K. Typical bio is ???-246K.

- Helium icy is 0.39g-0.55g.
- Helium rocky ice is 0.55g-0.90g.
- Helium rocky is roughly 1.19g-1.92g.
- Helium HMC is 0.97g-2.30g.
- Helium Metal-rich has insufficient data.
- Helium (rocky) ice is 20K-20.88K
- Helium rocky is 20K-73.1K
- Helium icy has a pressure of 0.030atm-0.1atm

- Methane icy is 0.02g-0.06g.
- Methane rocky ice is 0.03g-0.07g.
- Methane rocky is 0.04g-0.81g.
- Methane HMC is 0.05g-1.01g.
- Methane-rich rocky is 0.42g-0.75g.
- Methane-rich HMC is 0.46g-0.82g.
- Methane icy has a pressure of 0.028atm-0.1atm
- Methane rocky ice has a pressure of 0.0257atm-0.1atm
- Methane rocky has a pressure of 0.001atm-0.1atm
- Methane HMC has a pressure of 0.001atm-0.1atm
- Methane-rich rocky has a pressure of 0.011atm-
- Methane-rich HMC has a pressure of 0.009atm-0.1atm
- Methane (rocky) ice has a surface temperature of 82K-108.7K.
- Methane rocky has a surface temperature of 67K-166K.
- Methane HMC has a surface temperature of 67K-165K.
- Methane-rich rocky/HMC has a surface temperature of 72.7K-166K.

- Neon icy is 0.26g-0.7g. Typical bacteria has ???-0.610g.
- Neon rocky ice is 0.34g-0.81g. Typical bacteria has ???-0.610g.
- Neon rocky has insufficient data.
- Neon HMC is 0.63g-0.88g.
- Neon-rich icy is 0.25g-1.21g. Typical bacteria has ???-0.610g.
- Neon-rich rocky ice is 0.30g-1.28g.
- Neon-rich rocky is 0.34g-1.22g.
- Neon-rich HMC is 0.37g-1.41g.
- Neon icy has a pressure of 0.001atm-0.01131atm
- Neon rocky ice has a pressure of -0.0075atm
- Neon HMC has a pressure of -0.00273 atm
- Neon-rich all have a pressure of 0.001atm-0.1atm.
- Neon (rocky) ice has a surface temperature of 20K-61.8K
- Neon HMC has a surface temperature of 23.6K-53.1K.
- Neon-rich icy has a surface temperature of 20K-96.1K.
- Neon-rich rocky ice has a surface temperature of 20K-93.9K.
- Neon-rich rocky has a surface temperature of 20K-150K.
- Neon-rich HMC has a surface temperature of 20K-243K.

- Nitrogen icy is 0.19g-1.21g.
- Nitrogen rocky ice is 0.22g-0.46g.
- Nitrogen rocky is 0.04g-0.69g.
- Nitrogen HMC is 0.15g-0.80g.
- All Nitrogen have a pressure of 0.001atm-0.1atm
- Nitrogen icy has a surface temperature of 50K-91K.
- Nitrogen rocky ice has a surface temperature of 50K-85K.
- Nitrogen rocky has a surface temperature of 42K-260K.
- Nitrogen HMC has a surface temperature of 43.9K-316K.

- Oxygen icy is 0.23g-0.45g.
- Oxygen rocky ice is 0.28g-0.50g.
- Oxygen rocky is 0.35g-0.73g.
- Oxygen HMC is 0.35g-0.83g.
- Oxygen icy has a pressure of 0.012atm-0.1atm
- Oxygen rocky has a pressure of 0.012atm-0.1atm
- Oxygen rocky ice has a pressure of 0.015atm-0.1atm
- Oxygen HMC has a pressure of 0.012atm-0.1atm
- Oxygen icy has a surface temperature of 143K-242K.
- Oxygen rocky ice has a surface temperature of 147K-242.1K.
- Oxygen rocky has a surface temperature of 148K-245.85K.
- Oxygen HMC has a surface temperature of 132K-245.85K.

- Sulphur Dioxide icy is 0.18g-0.33g.
- Sulphur Dioxide rocky ice is 0.21g-0.43g.
- Sulphur Dioxide rocky is 0.04g-0.89g.
- Sulphur Dioxide HMC is 0.05g-1.01g.
- Sulphur Dioxide Metal-rich has insufficient data.
- All Sulphur Dioxide have a pressure of 0.001atm-0.1atm
- Sulphur Dioxide icy has a surface temperature of 143K-314.6K.
- Sulphur Dioxide rocky ice has a surface temperature of 146.9K-314.6K.
- Sulphur Dioxide rocky has a surface temperature of 20K-500K.
- Sulphur Dioxide HMC has a surface temperature of 132K-500K.

- Water rocky is 0.04g-0.056g.
- Water HMC is 0.04g-0.064g.
- Water-rich icy is 0.31g-0.51g.
- Water-rich rocky ice is 0.38g-0.52g.
- Water rocky has 0.0506atm-0.1atm
- Water HMC has 0.0499atm-0.1atm
- Water-rich icy has a pressure of 0.0048atm-0.1atm
- Water-rich rocky ice has a pressure of 0.0057atm-0.1atm
- Water rocky has a surface temperature of 249K-452.3K. Typical bio is 390K-???
- Water HMC has a surface temperature of 240K-452.1K.
- Water-rich icy has a surface temperature of 194K-340K.
- Water-rich rocky ice has a surface temperature of 200K-327.2K.