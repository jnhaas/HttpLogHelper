# HttpLogHelper
Helps to add HTTP post data in the IIS HTTP log files

This is an IIS *HttpModule* that need to be registered in a IIS web application to log POST data in the Query field of the IIS HTTP log file

How to register it with in application web.config file once the assembly HttpLogHelper.dll has been copied in the bin folder.

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
