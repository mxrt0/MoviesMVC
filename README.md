# MoviesMVC
Movies website with CRUD functionality where users can search for, as well as filter/sort through records. Supports dynamic fetching of IMDb rating and starring actors provided the movie is real.
Developed with ASP.NET MVC + SQL Server.

## Given Requirements
* This is an application to manage information about movies.
* Users should be able to Add, Delete, Update and Read from a database, using website as the front-end.
* You need to use Entity Framework, raw SQL isn't allowed.
* There needs to be a search functionality where movies can be searched by name.
* SQL Server should be used (as opposed to SQLite)
* Add your personal touch with different colors, buttons, etc. The more the better.
* You can't leave any unused code from the tutorials. Make sure you remove any unused controllers, unnecessary comments and whatever else you find that doesn't abide to professional standards.

## Features
* Simple UI
* CRUD functionality for movies
* Favourites functionality (star button toggle)
* Filtering by genre, search filter, favourites-only filter
* Sorting by name, release date, etc. (clickable headers)
* Dynamic fetching of IMDb rating and starring actors provided movie exists (via OMDb API)
* Export to Excel functionality
* Pagination capabilities
* Automatic total price calculator

## Challenges
* Bootstrap/CSS styling
* Getting used to server side rendering
* Combining SSR w/ client-side code execution
* Reducing cluttered code
* Page handlers are difficult to set up
* ASP.NET automatic model and page binding is a little hard to follow sometimes

## Lessons Learned
* MVC is the most intuitive pattern for creating interactive web apps with ASP.NET
* EPPlus is a must for working with Excel
* Attributes are key

## Areas To Improve
* Front-End design and overall styling
* Razor Views and how they interact with controller actions

## Resources
* MS Docs for first ASP.NET MVC Project

## Configuration Instructions
* First of all, users need a valid API Key in order to be able to use the OMDb API (to support rating/actors fetching).
* If you don't already have one, navigate to the OMDb API website and follow the in-site instructions to generate one for yourself.
* Then, ensure a SQL Server database exists for the purpose of the app. You can use a local or a remote one, but configure it so that you have a valid connection string that points to it.
* Then, navigate to the API project folder, and locate the 'appsettings.json.example' file. Replace the placeholder values for the connection string and API key with your personal ones and remove the '.example' extension so it acts as an actual config file for the application.
* You should be all set!

