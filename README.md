# MayDay Button
Simple floating form for my store's registers.

# Current features:

Resets printers and printer queue

Restart Adobe - Our POS causes it to freeze all the time

Restart Flowhub - Our POS, is literally a POS

Requires a password to close - Because one of my coworkers "Didn't like it"

Mayday Parade

Big read button to cause panic

Can remotely update itself by sending the string "update" via tcp to the IP address of the machine. 
File path for the updated file will need to be manually adjusted.

Make sure to adjust the IP addresses as needed

"But Joe having TCP open on your network to a program with admin privilages is unsafe! Why Would you ever do that?!"
"UwU, but no tbh I don't really care. Our network doesn't contain any sensitive information. And all that our devices do, is launch a web app and play spotify. If someone does decide to do malicious things with this, go for it. Please. My job is boring I want something to do."


#Email Config
Adjust in Form1.cs your Email.config file location
File should be structured as such
[Email]
[App Specific Password]
[Destination Email]

#Slack config
Adjust in Form1.cs your Slack.config file location
In the file each line will be the address to your direc message to your tech.
Can be changed to whatever but that's how I'm using it.

# Update:
Removed need to have the program run as admin. In doing so the root folder of the application was moved to C:\MayDayButton\ which is where updates and the lgos are stored normally anyways. Program will only run as admin if it needs to adjust the startup registry key. If you want to change the location of the program just make sure to adjust the locations as needed in the code.

In doing this hopefully the reliability of it starting up everytime will be fixed. Fuck windows 10.

This change might not always work 100% of the time. In my testing, unless you manually update the program it may need to be updated twice. This is just how I chose to handle removing the old program from the startup folder. If it is not startting up properly check the registry key
\\Local Machine\\Software\\Microsoft\\Windows\Current Version\\Run\\MayDayButton and make sure that it's set to the correct Folder path (C:\MayDayButton\MayDayButton.exe)
