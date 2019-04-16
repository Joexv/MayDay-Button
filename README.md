# MayDay Button
Simple floating form for my store's registers.

# Current features:

Resets printers and printer queue

Restart Adobe - Our POS causes it to freeze all the time

Restart Flowhub - Our POS, is literally a POS

Mayday Parade

Big read button to cause panic

Can remotely update itself by sending the string "update" via tcp to the IP address of the machine. 
File path for the updated file will need to be manually adjusted.

Make sure to adjust the IP addresses as needed

"But Joe having TCP open on your network to a program with admin privilages is unsafe! Why Would you ever do that?!"
"UwU"


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
