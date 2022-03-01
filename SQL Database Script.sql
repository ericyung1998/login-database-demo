CREATE DATABASE logindb;
USE logindb;	
CREATE TABLE users(userid VARCHAR(6),
					username VARCHAR(20),
                    password VARCHAR(48),
                    firstName VARCHAR(40),
                    lastName VARCHAR(40),
                    email VARCHAR(50));
SELECT * FROM users;
