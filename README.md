#IFCtoJSON converter

IFC->JSON converter is designed to convert the IFC (industry foundation classes) datafile to JSON format with the help of IFC Schema Specifications.

Wikipedia describes IFC as datamodel with intention to describe architectural, building and construction industry data but even for a relativily small buildings amount of required data grows very fast. In order to contain data managable this converter was created. It assigns to values in IFC datafile apropriate descriptions and converts to clearer view in json database. It is also possible to extend converter to include nested objects which provides clearer relationship between IFC objects but creates significantly larger files.
