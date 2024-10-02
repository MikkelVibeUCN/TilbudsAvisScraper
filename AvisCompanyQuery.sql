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
    CONSTRAINT FK_Price_Product FOREIGN KEY (ProductId) REFERENCES Product(Id)
);

insert into company(Name) values ('Rema1000');

alter table avis add ExternalId int