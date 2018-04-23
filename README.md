# SimpleGoogleAnalyticsForUnity


Tired of importing multiple megabytes of libraries and using extra build steps for every platform just to get Google Analytics running in your Unity project?

How about all the namespace conflicts with all other Google services?

Here's a minimal C# only implementation of the basics at about ~300 rows.


One trade-off is that Google Analytics seems to ignore the "ds"(platform) value and instead use the HTTP user agent. It counts Android devices as mobile and everything else as desktop.

Now with options to anonymize both device id and ip.
