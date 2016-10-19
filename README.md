# EASOL Ed-Fi to OneRoster Utility



The EASOL Ed-Fi to OneRoster Utility provides the ability to easily export data stored in an Ed-Fi Operational Data Store to the IMS  [One Roster](https://www.imsglobal.org/lis/imsOneRosterv1p0/imsOneRosterCSV-v1p0.html) CSV format.

<img src="http://www.ed-fi.org/wp-content/themes/ed-fi/assets/i/ed-fi-branding-logo.png" width="102"/>  <img src="https://s31.postimg.org/dijcpqdnv/full_IMSglobalregistered.jpg" width="150"/>

The application has passed conformance testing by IMS GLOBAL. See [this] (https://www.imsglobal.org/compliance/easol-ed-fi-oneroster-tool) link for more information.

Please email us at ef2or@easol.org if you have any questions,  problems or suggestions

## Use Cases

This utility includes powerful functionality to enable data export from the Ed-Fi Data Standard to OneRoster v1.0/1.1format. The use cases supported include:
- Converting data from the Ed-Fi Data Standard v2.0 to One Roster v1.0/v1.1, with the ability to dynamically view and filter the dataset using domain facets.
- Simple integration with an Ed-Fi ODS/API instance as the data source, via providing a URL, key and secret.
- Downloading export packages, structured as per the IMS OneRoster specification.
- Support for creating and managing export templates for reuse and scheduled download by vendors.
- Ability for vendors to download data sets in an automated, scheduled manner.
- Ability to log and track application usage and template download activity across all of the vendors who were provided with an auth key. 

## Functionality

### Export Utility

An administrator defines filters to retrieve a data-set from the Ed-Fi API, which is converted and packaged into One Roster v1.0 format. It is possible to preview the data on the page and  download it as an archive.

![Export Utility](https://cloud.githubusercontent.com/assets/5213372/18935012/ddfc8b96-85e5-11e6-8981-427c8ec26021.png)

### Templates

Templates allow third-party vendors to access data remotely with pre-defined filters.  Each template is assigned with a unique token (which must be specified in the HTTP header) and can be retrieved by using a cURL command such as:
```
curl -o filename.zip  https://domain/Export/$template_id -H "Token:<template token>"
``` 

![Templates](https://cloud.githubusercontent.com/assets/5213372/18935057/278890de-85e6-11e6-881a-e540db44f173.png)
### Users
The current version of the application supports a single user (admin), which is configured during the application set-up. In case if the password is lost, the admin should manually connect to the database and change the `PasswordHash` field for the only row in `AspNetUsers` table to `AOx91piOvWdXB+CDudmdhPxBjAZz5aiTtE6xklBQPM4f6ayntGp3psQtTSV4CfeqDw==`. This would change the password to `aU8n&9$nw#72gFb&2ib%j3`. Afterwards, we strongly suggest to change it by using Settings section of the application


Additionally, to improve security an auto-lock feature is implemented, which is enabled after 5 unsuccessful attempts.The auto-lock is disabled after 1 hour or by changing a value in the database.


### Logs

Logs allow auditing of application operations and troubleshooting if errors are encountered during data retrieval.

### Settings

Settings allow the Administrator to do the following:
* Change password
* Change Ed-Fi ODS API connection details
* Manage Ed-Fi to One-Roster related mapping parameters
* Change database connection details


###### Important - with higher amounts of data the application gets slower. The following enhancements described in this [ticket] (https://tracker.ed-fi.org/browse/ODS-932) will be applied in order to improve it.

## Getting Started

The instructions below will describe how to setup and deploy **EASOL Ed-Fi to OneRoster Utility** on a selected Windows Machine and configure it to be ready to operate.


### Prerequisites

1. [Ed-Fi ODS API v2.1](https://techdocs.ed-fi.org/display/ODSAPI21/) (or higher) - configured and running instance
2. IIS 8.5+
3. SQL Server 2012+ 
4. [.NET 4.5 Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=42643)

Visual Studio 2015 is required if building the solution from source code.

Important Information for Configuring Application Pool Identities and Permissions:
* http://www.iis.net/learn/manage/configuring-security/application-pool-identities
* http://stackoverflow.com/questions/5437723/iis-apppoolidentity-and-file-system-write-access-permissions

***

### Installation (deployment)

The process of deployment the system on a machine.

#### Download the Source Code
Download the `release` folder and place it into selected location.

#### Add the Application into IIS 

Open IIS Manager, Right-Click on `Default Web Site` and select `Add Selection`. Place `EDFI2OR` into the `Alias` and the path to unzipped folder into `Physical Path` field.

#### Validate the application

Validate whether the application is working by navigating to http://localhost/EDFI2OR

#### Create an empty database

This application requires a database to be created separately. Create an empty database with any name for this purpose.
(The database can be local, remote, Azure, AWS, etc.)

#### Configure Application connection to your database

The first screen on the application is the Database Configuration. Enter your details and submit the form. It might take a while before the system boots up.

![Initial Set-up](https://s31.postimg.org/d4w1ar05n/Screen_Shot_2016_08_01_at_19_31_46.png)

#### Configure Administrator user account and connection to your Ed-Fi ODS API Project
The next screen after Database Configuration is Ed-Fi ODS API and User set-up.
* The user name is your email address.
* The password needs to be at least 6 characters and have at least one number or character.


![Registration](https://s31.postimg.org/m2b0hxuaz/registration2.png)

#### Notes
* The application pool assigned to the Web Site hosting the Web Application must be running on .NET 4 and Integrated Pipeline.
* Verify your Database is configured to allow Remote Connections
* If using Integrated Security, verify the user running the application pool has permissions to access the database.


***

### Building & Testing
The process of building the project on a machine together with testing. This would be required if there are required changes to be done for the application.

#### Launch Project in Visual Studio
Open Visual Studio 2015 and open `EF2OR.sln` project. Ensure that the `EF2OR` project is set as a startup project. Click `Build` and then `Rebuild Solution`


#### Running the tests
Tests are kept in a separate EF2OR.Tests Visual Studio Project. It's suggested to run those after doing any of the source code changes. The tests are using a separate database, which should be defined. The responses from Ed-FI API ODS are mocked, so you don't need to enter the credentials in this project.

* With the `EF2OR.sln` solution open find the file `app.config` in the project `EF2OR.Tests`.
* Modify the connectionstring named “DefaultConnection”, with the connectionstring used to connect to the database you want to test with.
The default is:
```xml
<add name="DefaultConnection" connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ED2OR;Integrated Security=SSPI" providerName="System.Data.SqlClient" />
```
* Save, Rebuild All, and then go to `Test -> Windows -> Text Explorer`
![Test Explorer](https://s32.postimg.org/ss668ieph/Untitled.png)
![Test Explorer](https://s31.postimg.org/r8x52h4jf/image.png)
* Click `Run All`

***


## Contributing

Community contributions to this application will keep it healthy and active.  We strongly welcome pull requests with new feature enhancements and bug fixes.

## License

This project is licensed under the Apache 2.0 license - see the [LICENSE.md](LICENSE.md) file for details
