# Survival Shooter Upgraded

![A screenshot of the game showing the player shooting at a Zombunny](.github/screenshot.png)

This is a copy of the **Survival Shooter** project upgraded for newer versions
of Unity. It is provided as a part of the instructional content at AIE Seattle.

All tutorial content and problems are implemented in their entirety in the
Unity project contained within.

The original copy of the Survival Shooter is available for free from Unity
Technologies on [Unity Learn][U3D_SurvivalShooter].

[U3D_SurvivalShooter]:https://learn.unity.com/project/survival-shooter-tutorial

## Differences From Tutorial Project

Some differences are present when compared to the original tutorial project:

Property        | Original                           | Latest
----------------|------------------------------------|------------------------
Version         | Unity 5                            | Unity 20XX (see [Building](#building))
Render Pipeline | Built-in (BIRP)                    | Universal (URP)
Lighting        | Face Light Only                    | Flashlight + Face Light
Global Lighting | Single Directional Light + Ambient | Ambient Only

> :question: **Why use the Universal Render Pipeline?**  
> The Survival Shooter project as originally released for Unity 4.6 was not
> designed as an ultra high-fidelity experience.
>
> To target scalability, the Universal Render Pipeline was chosen for its modest
> feature-set and resource load.

## Building

This project is tested with **Unity 2020.3.5f1** but may work with newer versions.

After cloning this project, add the working directory to Unity Hub and open the
project. If the corresponding version is not installed on your machine, accept
the prompt from Unity Hub at the bottom of your screen to proceed with
installation.

Once installation is complete, retry opening the project from Unity Hub.

## License

Copyright (c) 2018-2022 Academy of Interactive Entertainment

Unless stated otherwise, the contents of this project are licensed under the MIT
License.

Check the [LICENSE](LICENSE) for more details.

## Thirdparty

"Survival Shooter" by Unity Technologies is licensed under Apache License 2.0.

Full notices for all third-party works are enumerated in the [THIRDPARTY](THIRDPARTY) file.
