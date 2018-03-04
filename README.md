# HoldYourHorses
.Net Rate limiting Action filter. Effective but not perfect by a long shot. Used in memory cache to store previous calls.

Limits the user by their Authorization header. Easily changed to be limited by any request value.

Main file to copy
https://github.com/tharris29/HoldYourHorses/blob/master/HoldYourHorses/ThrottleAttribute.cs


Add the following Action filter to the controller. Below example would limit 20 calls for every 60 seconds. If no values are set default values are used.

```csharp
[Throttle(CallsPerRateLimitSeconds =20,RateLimitSeconds =60)]
```
