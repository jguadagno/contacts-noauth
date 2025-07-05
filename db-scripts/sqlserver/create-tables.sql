USE Contacts
GO

create table AddressTypes
(
    AddressTypeId int identity
        primary key,
    Type          nvarchar(max),
    Description   nvarchar(max)
)
go

create table Contacts
(
    ContactId    int identity
        primary key,
    FirstName    nvarchar(max),
    MiddleName   nvarchar(max),
    LastName     nvarchar(max),
    EmailAddress nvarchar(max),
    ImageUrl     nvarchar(max),
    Birthday     datetime,
    Anniversary  datetime
)
go

create table Addresses
(
    AddressId        int identity
        primary key,
    StreetAddress    nvarchar(max),
    SecondaryAddress nvarchar(max),
    Unit             nvarchar(max),
    City             nvarchar(max),
    State            nvarchar(max),
    Country          nvarchar(max),
    PostalCode       nvarchar(max),
    AddressTypeId    int
        constraint FK_Addresses_AddressTypes_AddressTypeId
            references AddressTypes
            on delete cascade,
    ContactId        int
        constraint FK_Addresses_Contacts_ContactId
            references Contacts
            on delete cascade
)
go

create index IX_Addresses_AddressTypeId
    on Addresses (AddressTypeId)
go

create index IX_Addresses_ContactId
    on Addresses (ContactId)
go

create table Log
(
    Id          int identity
        constraint [PK_dbo.Log]
            primary key,
    MachineName nvarchar(50)  not null,
    Logged      datetime      not null,
    Level       nvarchar(50)  not null,
    Message     nvarchar(max) not null,
    Logger      nvarchar(250),
    Callsite    nvarchar(max),
    Exception   nvarchar(max)
)
go

create table PhoneTypes
(
    PhoneTypeId int identity
        primary key,
    Type        nvarchar(max),
    Description nvarchar(max)
)
go

create table Phones
(
    PhoneId     int identity
        primary key,
    PhoneNumber nvarchar(max),
    Extension   nvarchar(max),
    PhoneTypeId int
        constraint FK_Phones_PhoneTypes_PhoneTypeId
            references PhoneTypes
            on delete cascade,
    ContactId   int
        constraint FK_Phones_Contacts_ContactId
            references Contacts
            on delete cascade
)
go

create index IX_Phones_ContactId
    on Phones (ContactId)
go

create index IX_Phones_PhoneTypeId
    on Phones (PhoneTypeId)
go
