# SrvSurvey

SrvSurvey is an independent third-party application for use with [Elite Dangerous](https://www.elitedangerous.com) by [Frontier Developments](https://frontier.co.uk). It provides on-screen assistance when a player is near a planet in the SRV, on foot or in ships. It has 3 main functions:
- **Organic scans:** Track the location of organic scans and the distance required for the next scan.
- **Ground target tracking:** Aiming guidance towards latitude/longitude co-ordinates.
- **Guardian sites:** Track visited areas and the locations of items within Guardian ruins and structures.

The application works by analyzing journal files written by the game when played on a PC, tracking the location of the player at the time of various events. It uses this information to render overlay windows atop the game, updated in real-time as the player moves about. For the most part the application is fully automatic, players need to start the application and then just play the game. It will remain hidden until triggered by events from the game.



# Installation

Srv Survey is distributed two ways:

- An unsigned build is available through [GitHub releases](https://github.com/njthomson/SrvSurvey/releases), updated frequently. Simply download the .zip file and run `setup.exe`. You may need to manually uninstall a previous version, but you will not lose your prior settings or surveys.

- An official signed build is available through [the Windows App Store](https://www.microsoft.com/store/productId/9NGT6RRH6B7N). This is updated less often, when new features are ready or critical bugs are fixed.

Srv Survey is not related to [EDMC](https://github.com/EDCD/EDMarketConnector/wiki) (Elite Dangerous Market Connector) and is not installed as one of its plugins.


# General usage:

Srv Survey uses various overlay windows that will appear automatically at suitable times, like when arriving at Guardian Ruins or when scanning for biological signals, and will close automatically when no longer useful. These can be disabled individually in settings if specific ones are not wanted.

The main window of Srv Survey is not really the interesting part but provides access to the features below. It otherwise shows a brief summary of your game status and can be safely ignored. It is worth looking in Settings once in a while as there are further abilities in there not enabled by default.

![image](https://github.com/njthomson/SrvSurvey/assets/3903773/dc4eb608-cbb5-4f30-8b53-5eee563a0c51)

On the main window:

- Set target lat/long co-ordinates via `Set target` button, see Tracking ground targets.

- Set the central system for Spherical searching via `Sphere limit` button, see Spherical searches.

- View a table of all known Guardian Ruins via `All Guardian Ruins`, or survey maps from specific sites via `Ruins Maps`.

- Switch between showing ruins maps or aerial screenshot assistance.

# Organic scanning:

When approaching a planet or moon with biological signals, a summary panel will appear top center. This shows the count, the names of biological organisms detected and how many have been analyzed. Items in orange have been scanned, items in blue have not been scanned.

Upon the first scan with the Genetic Sampler tool, the summary changes to show the name of the current biological signal, filled circles for how many scans have been completed and most importantly, how far a player needs to travel for the next sample with sufficient genetic diversity.
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/d91badbc-30f7-4184-bb79-8a349e26d984)

Upon landing, disembarking or launching the SRV a second panel will appear on the right side. This shows distances and directions from prior scans, your ship and your SRV. The circles will change color once you have travelled away far enough from a scan.
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/b20670ef-2090-480e-91c6-96beec6dfaaa)


# Surveying Guardian Ruins

The goal for surveying Guardian Ruins is to measure site and relic tower headings, plus record which specific items and relic towers are present or missing. Srv Survey triggers from in-game actions to capture survey data so you do not need to put down your game controller, hotas, etc. This works by toggling cockpit mode or switching between fire-groups, requiring that 3 fire-groups be created for your Ship for SRV. It does not matter what those fire-groups do.

**IMPORTANT WARNING**: It is highly recommended to keep the landing gear **down** whilst surveying. Accidentally hitting boost so close the ground is usually a terminal and expensive mistake.

## Measuring Site headings:

If not already known the heading of a Guardian ruins can be measured by aligning your ship with the specific buttress shown per site:
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/929b29b8-bfcd-459e-ad0c-fcae197d4e9d)

To help align your ship with precision Srv Survey will overlay an aiming grid in the center of your screen. Align the inner vertical bars with the **back** of the buttress and the outer bars with the **front** of the buttress, using parallax check the left and right sides are equally spaced. You may need to shift your ship left or right a little to make them line up evenly.

Once in position toggle cockpit mode twice, one second apart, and Srv Survey will record the current heading and remove the aiming grid.
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/14396ed9-e269-4833-9f41-b6aeffe4b3fc)

## Measuring Relic Tower headings:
`coming soon`
  
## Surveying which items are present or missing:

To record which items or Relic towers are present or missing move your ship or SRV (always the green circle in the center of the map) close to each **blue** icon, getting close enough for a blue circle to appear around it. The expected type and name will be shown at the bottom of the map. Some items are quite close together requiring care to ensure you're recording the right one.
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/4c691e21-50ff-41a1-a739-83601cff1bf3)

Srv Survey knows the location from journal files only as fast as Elite Dangerous updates them, which can be slow according to documentation ...

> The latitude or longitude need to change by 0.02 degrees to trigger an update when flying, or by 0.0005 degrees when in the SRV

... which is unfortunate when surveying. Toggling ship functions like ship lights or night vision will force Elite Dangerous to update those files and Srv Survey will then update accordingly.

Switch fire-group depending on if the Relic Tower or Item is present, missing or empty and toggle cockpit mode twice, one second apart. The exact timing depends on how quickly Elite Dangerous updates journal files, so the text `(toggle cockpit mode once to set)` will turn blue when the first toggle is detected and orange again after 2 seconds.
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/569fc13e-b226-4a5e-80c4-7c43ed75af58)

Change the fire-group as follows.

- Fire-group A : Item or Relic Tower is **present** at this Ruins.
- Fire-group B : Item or Relic Tower is **missing** at this Ruins.
- Fire-group C : Choose this if the Item is missing but there is a glowing "puddle" on the ground where it would have been. (This is not a valid option for Relic Towers)

When choosing fire-group C it is highly recommended to relog at that location to confirm the Item really is missing. Empty puddles are uncommon and it is common for the item in question simply to roll off before you arrive. If there is no Item immediately visible after logging back in - the puddle really is empty.

The map icons change color based on their status. Relic Towers icons are larger, Items smaller.
- Solid orange : Item or Relic Tower is **present**.
- Solid red : Item or Relic Tower is **missing**.
- Solid yellow : Empty puddle with missing Item.
- Dotted/broken blue : status unknown.

The top of the map shows the count of how many Relic Towers and Item locations still need to be surveyed, which will change to orange when the survey is complete.

As the game allows us to scan Relic Towers with a Composition Scanner, a Relic Tower will be marked as **present** if it is scanned.

![image](https://github.com/njthomson/SrvSurvey/assets/3903773/f320aef2-fd63-44d5-a713-d5ccfef5554f)


# Screenshot conversions:

Whilst similar abilities are available in other companion apps for Elite Dangerous, Srv Survey optimizes the process for Guardian sites by:

- Stamping site details into the corner of images, eg: Ruins number, lat/long, heading, etc.
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/7c6c980f-2ed1-48ae-a243-9fd7e84d0ec8)

- Including Ruins number in filenames.

- Using Aerial assistance : shows a guide for keeping your ship aligned above the origin of a ruins as you ascend to a suitable altitude. This makes it easy to take whole site screenshots from a consistent position, accurate within meters. Once you reach that target altitude, visual guides appear to help align the external camera view exactly over the site in question.
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/26cc2a80-c8ab-452b-bccf-168935e68dca)

- Making a copy of aligned aerial screenshots in folders per type. Hence you will have a folder `Aerial Alpha` with only Alpha site images, and another for Beta, Gamma, etc respectively.
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/6662b415-54b3-47e9-8b85-5da08e6ed96b)

- As Alpha sites are less square than the others, there is an option to rotate them. This takes advantage that monitors are typically aligned landscape not portrait, enabling higher quality images that are then rotated to be portrait:
![image](https://github.com/njthomson/SrvSurvey/assets/3903773/2cad70af-b0d4-4b08-9352-5c46baeab051)


