-- Create Company table
use TilbudsAvisDB
CREATE TABLE Company (
    Id int identity(1,1) primary key not null, 
    Name VARCHAR(255)
);

-- Create Avis table with a foreign key to Company
CREATE TABLE Avis (
    Id int identity(1,1) primary key not null, 
    ValidFrom DATE, 
    ValidTo DATE, 
    CompanyId INT,
	ExternalId INT,
    CONSTRAINT FK_Avis_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id)
);

-- Create Page table with a foreign key to Avis
CREATE TABLE Page (
    Id int identity(1,1) primary key not null, 
    PdfUrl VARCHAR(255), 
    PageNumber INT, 
    AvisId INT,
    CONSTRAINT FK_Page_Avis FOREIGN KEY (AvisId) REFERENCES Avis(Id)
);

-- Create Product table
CREATE TABLE Product (
    Id int identity(1,1) primary key not null, 
    ExternalId INT, 
    Name VARCHAR(255), 
    Description VARCHAR(255), 
    ImageUrl VARCHAR(255)
);

-- Create Price table with a foreign key to Product
CREATE TABLE Price (
    Id int identity(1,1) primary key not null, 
    ProductId INT, 
    Price FLOAT, 
    CONSTRAINT UQ_Product_Avis UNIQUE (productid, avisid)
    CONSTRAINT FK_Price_Product FOREIGN KEY (ProductId) REFERENCES Product(Id)
);

CREATE TABLE APIUser (
	Id int identity(1, 1) primary key not null,
	"Role" varchar(150) not null,
	PermissionLevel int not null,
	Token char(66) not null
)

CREATE TABLE NutritionInfo (
    productId INT PRIMARY KEY,

    EnergyKJ FLOAT NOT NULL,        
    FatPer100G FLOAT NULL,           
    SaturatedFatPer100G FLOAT NULL,  
    CarbohydratesPer100G FLOAT NULL, 
    SugarsPer100G FLOAT NULL,         
    FiberPer100G FLOAT NULL,          
    ProteinPer100G FLOAT NULL,        
    SaltPer100G FLOAT NULL,           

	CONSTRAINT FK_Product_Nutrition FOREIGN KEY (productId) REFERENCES Product(Id)
);

