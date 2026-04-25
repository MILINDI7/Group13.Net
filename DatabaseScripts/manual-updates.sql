ALTER TABLE OrganizerProfiles
ADD OrganizerSharePercentage DECIMAL(5,2) NOT NULL CONSTRAINT DF_OrganizerProfiles_OrganizerShare DEFAULT 90;

ALTER TABLE OrganizerProfiles
ADD AdminSharePercentage DECIMAL(5,2) NOT NULL CONSTRAINT DF_OrganizerProfiles_AdminShare DEFAULT 10;