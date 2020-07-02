# AirQualityVictoria

Twitter bot designed to post updates on air quality in Victoria directly 
from the Victorian EPA. Can be used as a 'daily weather check' for 
people suffering respiratory conditions as a result of bushfire smoke 
and pollution in Victoria.

This bot runs as a backround task on a raspi on my network. As it's written in C#, the pi runs an experimental build of windows 10 IOT.
I built this primarily to gain familiarity with the C# language, learn more about the fun stuff you can do with APIs, and in the pursuit of creating something at least mildly useful.  

You can find the bot here: https://twitter.com/airqualityvic 

## Key Files 

Bot logic is found [here](AirQualityVictoria/StartupTask.cs)
