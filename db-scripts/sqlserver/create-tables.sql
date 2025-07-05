USE Contacts
GO

create table AddressTypes
(
    AddressTypeId int identity
        primary key,
    Type          varchar(max),
    Description   text
)
go

create table Cache
(
    Id                         nvarchar(449)  not null
        primary key,
    Value                      varbinary(max) not null,
    ExpiresAtTime              datetimeoffset not null,
    SlidingExpirationInSeconds bigint,
    AbsoluteExpiration         datetimeoffset
)
go

create index Index_ExpiresAtTime
    on Cache (ExpiresAtTime)
go

create table Contacts
(
    ContactId    int identity
        primary key,
    FirstName    text,
    MiddleName   text,
    LastName     text,
    EmailAddress text,
    ImageUrl     text,
    Birthday     datetime,
    Anniversary  datetime
)
go

create table Addresses
(
    AddressId        int identity
        primary key,
    StreetAddress    text,
    SecondaryAddress text,
    Unit             text,
    City             text,
    State            text,
    Country          text,
    PostalCode       text,
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
    Type        text,
    Description text
)
go

create table Phones
(
    PhoneId     int identity
        primary key,
    PhoneNumber text,
    Extension   text,
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

create table __EFMigrationsHistory
(
    MigrationId    varchar(256) not null
        constraint PK___EFMigrationsHistory
            primary key,
    ProductVersion varchar(max) not null
)
go

