# HttpLogHelper
Allows to log HTTP post form data in IIS HTTP log files

This is an IIS *HttpModule* that need to be registered in a IIS web application to log POST data in the *cs-uri-query* field of IIS HTTP log files.

The binary file of the HttpModule can be downloaded from
[https://www.finalanalytics.com/downloads/HttpLogHelper.zip](https://www.finalanalytics.com/downloads/HttpLogHelper.zip)

Once the assembly *HttpLogHelper.dll* has been copied in the bin folder of the web application you need to register the *PostloggerModule* either through the IIS manager or through *web.config* of the application like below.

    <configuration>
		...
	    <system.webServer>
			...
	        <modules runAllManagedModulesForAllRequests="false">
	          ...
			  <add name="PostLoggerModule" type="HttpLogHelper.PostLoggerModule, HttpLogHelper" />
	        </modules>
			...
	    </system.webServer>
		...
    </configuration>
