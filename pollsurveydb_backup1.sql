-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: pollsurveydb
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `polloptions`
--

DROP TABLE IF EXISTS `polloptions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `polloptions` (
  `Id` char(36) NOT NULL,
  `PollId` char(36) NOT NULL,
  `OptionIndex` int NOT NULL,
  `OptionText` varchar(300) NOT NULL,
  `VoteCount` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_PollOptions_PollId` (`PollId`),
  CONSTRAINT `FK_PollOptions_Polls_PollId` FOREIGN KEY (`PollId`) REFERENCES `polls` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `polloptions`
--

LOCK TABLES `polloptions` WRITE;
/*!40000 ALTER TABLE `polloptions` DISABLE KEYS */;
INSERT INTO `polloptions` VALUES ('0ba0d56a-a3f0-4db5-8614-2d57c0814c7d','880da0aa-003a-4ab4-8a00-d2a700caf397',1,'2 ⭐ (Tệ)',0),('0e373aad-fe62-4946-95f1-9dd1912b5c74','a141be09-a6e9-428e-b739-c126405e99e0',4,'5',0),('0f49b159-35e7-40fe-b84d-d455db565773','27dc3d2c-4fa7-4171-a329-29c97791b324',4,'project manager',0),('0fee5679-935d-4a50-afe8-300181501df2','bb0dd604-df54-4280-9f73-cf0f75c1cbe6',2,'3 ⭐ (Bình thường)',0),('0ff9e556-2cdc-4da1-90be-ca92bc4b3625','eee785d7-0d49-49f7-9612-4d8e97c3805d',2,'765',0),('14c3c331-aedc-46e5-bd34-3f4de4484781','bb0dd604-df54-4280-9f73-cf0f75c1cbe6',3,'4 ⭐ (Tốt)',0),('23b8a80d-7917-4648-b610-977521e8cd8f','e6a0a010-a442-4b26-966c-0b48f6599974',4,'5 Sao',0),('2418dcfb-fe09-4fac-b31e-0e6bfb830e1b','a141be09-a6e9-428e-b739-c126405e99e0',5,'6',0),('2f5286b9-dfd8-4c56-9002-58363574878d','80651896-a162-4ce9-bfa0-443f68ee0923',0,'Có',0),('308abc95-cde2-4231-969e-d6391a44c78b','bb0dd604-df54-4280-9f73-cf0f75c1cbe6',0,'1 ⭐ (Rất tệ)',0),('317e4dc1-b7b2-4fc2-995b-4616bc0e2fb6','eee785d7-0d49-49f7-9612-4d8e97c3805d',3,'122123',0),('328a4c2b-3ef1-4d24-8f46-730a3ac4b26e','eee785d7-0d49-49f7-9612-4d8e97c3805d',5,'57787',0),('3d9f9cd7-56d5-4eb1-b6b0-d6f2741210b5','eee785d7-0d49-49f7-9612-4d8e97c3805d',1,'31232',0),('42b3a65a-6e14-47d9-a5bd-e28686112399','80651896-a162-4ce9-bfa0-443f68ee0923',1,'Không',0),('4976b14e-3bf5-44a9-82ea-752a53a41229','bb0dd604-df54-4280-9f73-cf0f75c1cbe6',4,'5 ⭐ (Rất tốt)',0),('4f0d927a-3f36-405b-b78b-3207de2bb038','27dc3d2c-4fa7-4171-a329-29c97791b324',3,'DSA',0),('590f76b7-8fd6-409f-a76f-b52cef2c4ce0','99e95c1f-1298-449d-8929-332891b1b77b',0,'bbbbbbbbbbbbbbb',1),('5b496705-8c37-4dcd-94f6-f1d2a7b11e9c','e6a0a010-a442-4b26-966c-0b48f6599974',3,'4 Sao',0),('5c2a1330-8b4c-4a8c-99da-2446eedb02a1','925bf87f-8b6c-49fb-bcc8-2ea151bf271d',2,'chủ nhật',0),('5e10c6b3-e583-4590-bb50-88d6c175266a','1b192c74-6752-4205-afb7-6211bb3b796c',0,'có',0),('618d0271-44ff-4823-9796-0e38965afcf0','ddee48c9-deed-4f88-911e-0c870ea6b8ce',0,'Có',0),('6d5ac110-7a63-40bb-ac0b-3658d9801403','27dc3d2c-4fa7-4171-a329-29c97791b324',2,'AWS',0),('70ca8758-b0f0-4727-ab41-357d05f40659','cad80aae-d344-4e90-8c63-117da6b222c2',0,'Có',0),('775209bc-99e5-4514-b9e7-1937c2086808','eee785d7-0d49-49f7-9612-4d8e97c3805d',0,'3213',0),('7943837a-fa09-4160-a8b3-345e70473a4c','99e95c1f-1298-449d-8929-332891b1b77b',1,'ccccccccccccccc',0),('79442a79-52a8-4a51-b2a7-dfcea9e5cce4','925bf87f-8b6c-49fb-bcc8-2ea151bf271d',0,'thứ 3',0),('7fd7922d-fc46-4fd1-a18e-34185c0e287c','eee785d7-0d49-49f7-9612-4d8e97c3805d',4,'5556',0),('83412e7c-6473-492f-8a3b-e51dd7d74a02','880da0aa-003a-4ab4-8a00-d2a700caf397',4,'5 ⭐ (Rất tốt)',0),('83cd57c3-bbc6-40ff-9ab6-f2c329487ce3','e6a0a010-a442-4b26-966c-0b48f6599974',0,'1 Sao',0),('892f57b4-5b0a-4f79-a3cb-846438156041','a141be09-a6e9-428e-b739-c126405e99e0',2,'3',0),('91dedc12-7cad-43a2-b752-f8a2d7284390','880da0aa-003a-4ab4-8a00-d2a700caf397',3,'4 ⭐ (Tốt)',0),('96556a6a-23fe-4fc8-afec-223e97cc30b2','e6a0a010-a442-4b26-966c-0b48f6599974',2,'3 Sao',0),('9b71318f-c52d-418a-96fd-57644596b8e3','880da0aa-003a-4ab4-8a00-d2a700caf397',0,'1 ⭐ (Rất tệ)',0),('af58031d-aed8-44f2-86d6-79383b2fbe3b','a141be09-a6e9-428e-b739-c126405e99e0',3,'4',0),('b1bd2cca-380b-465c-9fbe-7a4f966e1f3b','925bf87f-8b6c-49fb-bcc8-2ea151bf271d',1,'thứ 2',0),('be31181d-a7b3-4f36-83c0-36fc4084723f','27dc3d2c-4fa7-4171-a329-29c97791b324',0,'web1',0),('beb342e1-cd3d-4eea-974a-c7853a2cf639','bb0dd604-df54-4280-9f73-cf0f75c1cbe6',1,'2 ⭐ (Tệ)',0),('bedd8198-df52-4ad8-90d2-5ed6bbcb346e','1b192c74-6752-4205-afb7-6211bb3b796c',1,'không',0),('c96f42a6-355a-4c2c-bd48-5720c9582b20','ddee48c9-deed-4f88-911e-0c870ea6b8ce',1,'Không',0),('cccf4c21-4fcc-4154-8a5a-f7f7a6472bd8','a141be09-a6e9-428e-b739-c126405e99e0',0,'1',0),('d45c4152-dd30-4e5d-8644-5d8d9c0f50e7','2eab102a-88fe-48d6-b615-bbf2ec54eaba',1,'Không',0),('d5f7de46-fc87-47a7-adef-4f6f7948b6ca','27dc3d2c-4fa7-4171-a329-29c97791b324',1,'web2',0),('d863b111-3f7d-45c5-aeae-7be14cb88852','cad80aae-d344-4e90-8c63-117da6b222c2',1,'Không',0),('da2b373a-ccca-4b74-a2b2-52cf38474eba','27dc3d2c-4fa7-4171-a329-29c97791b324',5,'softwave',0),('e653a48f-b254-4a2d-8d9f-e3773e0bf78a','e6a0a010-a442-4b26-966c-0b48f6599974',1,'2 Sao',0),('e767871d-e75b-4b08-9c65-f53678f8e0b1','880da0aa-003a-4ab4-8a00-d2a700caf397',2,'3 ⭐ (Bình thường)',0),('ef028a47-3c7c-4f07-b154-1d4c8fbd07aa','2eab102a-88fe-48d6-b615-bbf2ec54eaba',0,'Có',0),('f2212879-60fe-4cd6-8318-4c1580d39af8','a141be09-a6e9-428e-b739-c126405e99e0',1,'2',0);
/*!40000 ALTER TABLE `polloptions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `polls`
--

DROP TABLE IF EXISTS `polls`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `polls` (
  `Id` char(36) NOT NULL,
  `Code` varchar(20) NOT NULL,
  `Title` varchar(300) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `QuestionType` int NOT NULL,
  `IsClosed` tinyint(1) NOT NULL,
  `ExpiresAt` datetime(6) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `CreatedByUserId` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Polls_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `polls`
--

LOCK TABLES `polls` WRITE;
/*!40000 ALTER TABLE `polls` DISABLE KEYS */;
INSERT INTO `polls` VALUES ('1b192c74-6752-4205-afb7-6211bb3b796c','TVvoio','điện thoại này đẹp không','',0,0,NULL,'2026-07-19 19:25:09.572051','8954a379-9ead-4b55-a163-6ccde7d97198'),('27dc3d2c-4fa7-4171-a329-29c97791b324','5HBFPY','bạn đang học môn gì','các môn học bạn được học tại trường đại học GreenWich',0,0,'2026-07-22 09:05:00.000000','2026-07-20 10:06:22.877909','8954a379-9ead-4b55-a163-6ccde7d97198'),('2eab102a-88fe-48d6-b615-bbf2ec54eaba','G18bg6','bạn thích tôi chứ','',1,0,NULL,'2026-07-19 19:23:32.783430','8954a379-9ead-4b55-a163-6ccde7d97198'),('7eefa7a8-a7f1-4605-b6cf-b87021d55b6d','b68tdy','hôm nay có thú vị không','có hoặc không',1,1,'2026-07-21 02:11:00.000000','2026-07-19 17:08:18.692043','8954a379-9ead-4b55-a163-6ccde7d97198'),('80651896-a162-4ce9-bfa0-443f68ee0923','6XzMYD','xin chao bạn','chào',1,0,'2026-07-20 08:51:00.000000','2026-07-19 17:05:04.871172','8954a379-9ead-4b55-a163-6ccde7d97198'),('880da0aa-003a-4ab4-8a00-d2a700caf397','JlroxE','bạn đang học môn gì','',2,0,'2026-07-23 15:09:00.000000','2026-07-20 10:08:01.556102','8954a379-9ead-4b55-a163-6ccde7d97198'),('925bf87f-8b6c-49fb-bcc8-2ea151bf271d','PuYfyD','hôm nay là thứ mấy','các thứ trong tuần bên dưới đây',0,0,NULL,'2026-07-19 16:45:26.605689','8954a379-9ead-4b55-a163-6ccde7d97198'),('99e95c1f-1298-449d-8929-332891b1b77b','wfYEFo','dddddddđ','qqqqqqqqqqqqq',0,0,'2026-07-19 16:35:00.000000','2026-07-19 15:36:03.648641',NULL),('a141be09-a6e9-428e-b739-c126405e99e0','80LLFK','dddddahhhhhhh','jjjjjjjjjjjjjj',0,0,'2026-07-22 09:48:00.000000','2026-07-20 09:43:28.180544','8954a379-9ead-4b55-a163-6ccde7d97198'),('bb0dd604-df54-4280-9f73-cf0f75c1cbe6','nrftb2','bạn có thích dịch vụ này không','dịch vụ bình chọn bạn đang sài',2,0,NULL,'2026-07-19 18:48:08.009568','8954a379-9ead-4b55-a163-6ccde7d97198'),('cad80aae-d344-4e90-8c63-117da6b222c2','AjGBt3','bạn có thích máy tính này không','',1,0,NULL,'2026-07-19 19:13:17.643540','8954a379-9ead-4b55-a163-6ccde7d97198'),('ddee48c9-deed-4f88-911e-0c870ea6b8ce','gepzf1','bạn đang học môn gì','ll',1,0,'2026-07-23 12:07:00.000000','2026-07-20 10:07:32.883676','8954a379-9ead-4b55-a163-6ccde7d97198'),('e6a0a010-a442-4b26-966c-0b48f6599974','fkpjNh','aaaaaaaaaaddddddddd','dddasdadadaaaaaaa',2,0,'2026-07-18 17:21:00.000000','2026-07-19 17:14:25.094410','8954a379-9ead-4b55-a163-6ccde7d97198'),('eee785d7-0d49-49f7-9612-4d8e97c3805d','bUd1be','bạn đang học môn gì','',0,0,'2026-07-31 10:21:00.000000','2026-07-20 10:21:31.317505','8954a379-9ead-4b55-a163-6ccde7d97198'),('f32682c6-ac3e-4b9d-9f7d-cb370948b8a2','XWCCOU','bạn đang học môn gì','iiiiii',3,1,'2026-07-23 15:08:00.000000','2026-07-20 10:08:42.178735','8954a379-9ead-4b55-a163-6ccde7d97198');
/*!40000 ALTER TABLE `polls` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `qnaquestions`
--

DROP TABLE IF EXISTS `qnaquestions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `qnaquestions` (
  `Id` char(36) NOT NULL,
  `PollId` char(36) NOT NULL,
  `QuestionText` varchar(500) NOT NULL,
  `VoterToken` varchar(100) NOT NULL,
  `Upvotes` int NOT NULL,
  `IsPinned` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UserId` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_QnAQuestions_PollId` (`PollId`),
  CONSTRAINT `FK_QnAQuestions_Polls_PollId` FOREIGN KEY (`PollId`) REFERENCES `polls` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `qnaquestions`
--

LOCK TABLES `qnaquestions` WRITE;
/*!40000 ALTER TABLE `qnaquestions` DISABLE KEYS */;
INSERT INTO `qnaquestions` VALUES ('05901b70-08b7-454d-9dc6-f6f0fbf70255','eee785d7-0d49-49f7-9612-4d8e97c3805d','dấd','voter_nm5ohbnts',1,1,'2026-07-20 10:22:01.903776','8954a379-9ead-4b55-a163-6ccde7d97198'),('3ba7247f-eaca-4959-b691-37a23fc89c58','880da0aa-003a-4ab4-8a00-d2a700caf397','li','voter_nm5ohbnts',1,1,'2026-07-20 10:08:24.775274','8954a379-9ead-4b55-a163-6ccde7d97198'),('5b7a6aed-8980-4488-800c-b1a1ca036268','1b192c74-6752-4205-afb7-6211bb3b796c','đấ','voter_nm5ohbnts',0,1,'2026-07-19 19:26:12.070934','8954a379-9ead-4b55-a163-6ccde7d97198'),('7ff1797c-c84b-4793-af4a-66f3c72ed694','925bf87f-8b6c-49fb-bcc8-2ea151bf271d','xin chào','voter_nm5ohbnts',19,1,'2026-07-19 16:47:47.761848','8954a379-9ead-4b55-a163-6ccde7d97198'),('cd2c9092-94fc-4471-95c5-7c1efed29959','99e95c1f-1298-449d-8929-332891b1b77b','hi','307d8c12dc7440b691ec2a691e0ee75f',13,0,'2026-07-19 15:40:13.430524',NULL),('de6c1631-d83c-43c1-a3e4-7035a263afa8','925bf87f-8b6c-49fb-bcc8-2ea151bf271d','mọi người ơi','voter_nm5ohbnts',2,0,'2026-07-19 18:47:03.659150','8954a379-9ead-4b55-a163-6ccde7d97198'),('ffae8348-9368-448a-927a-d899fd1bff57','f32682c6-ac3e-4b9d-9f7d-cb370948b8a2','tuyệt','voter_nm5ohbnts',1,0,'2026-07-20 10:09:13.011142','8954a379-9ead-4b55-a163-6ccde7d97198');
/*!40000 ALTER TABLE `qnaquestions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` char(36) NOT NULL,
  `Email` varchar(150) NOT NULL,
  `Username` varchar(100) NOT NULL,
  `PasswordHash` longtext,
  `AuthProvider` varchar(50) NOT NULL DEFAULT 'Local',
  `CreatedAt` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Users_Email` (`Email`),
  UNIQUE KEY `IX_Users_Username` (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES ('2e3d4968-f9b8-4624-b82c-ac92fd090610','nistl07062004@gmail.com','nistl07062004',NULL,'Google','2026-07-19 16:43:00'),('8954a379-9ead-4b55-a163-6ccde7d97198','hello07062004@gmail.com','ThanhDat','$2a$11$JlXnxJHNGO9rQVeCGeIUU.TN5j7gHWt7wyTh0p9psTCH06C69/hBK','Local','2026-07-19 16:43:41');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `votes`
--

DROP TABLE IF EXISTS `votes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `votes` (
  `Id` char(36) NOT NULL,
  `PollId` char(36) NOT NULL,
  `OptionIndex` int NOT NULL,
  `VoterToken` varchar(100) NOT NULL,
  `TextResponse` varchar(1000) DEFAULT NULL,
  `VotedAt` datetime(6) NOT NULL,
  `UserId` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Votes_PollId_VoterToken` (`PollId`,`VoterToken`),
  CONSTRAINT `FK_Votes_Polls_PollId` FOREIGN KEY (`PollId`) REFERENCES `polls` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `votes`
--

LOCK TABLES `votes` WRITE;
/*!40000 ALTER TABLE `votes` DISABLE KEYS */;
INSERT INTO `votes` VALUES ('0a561e4b-325c-4681-84dc-9865ece8a6e4','27dc3d2c-4fa7-4171-a329-29c97791b324',1,'voter_nm5ohbnts',NULL,'2026-07-20 10:06:36.784138','8954a379-9ead-4b55-a163-6ccde7d97198'),('1268cb87-64b7-4a42-9d74-45ee5d1ccd6e','bb0dd604-df54-4280-9f73-cf0f75c1cbe6',3,'voter_nm5ohbnts',NULL,'2026-07-19 18:48:16.404735','8954a379-9ead-4b55-a163-6ccde7d97198'),('177d9ca6-555e-4801-8976-1eccc87bc3d0','cad80aae-d344-4e90-8c63-117da6b222c2',0,'voter_nm5ohbnts',NULL,'2026-07-19 19:13:34.229862',NULL),('23787aad-d131-4725-9682-5071434a9706','1b192c74-6752-4205-afb7-6211bb3b796c',0,'voter_nm5ohbnts',NULL,'2026-07-19 19:25:23.003115',NULL),('2b9a46ff-97ab-4c83-97b3-3ffc365ffb38','925bf87f-8b6c-49fb-bcc8-2ea151bf271d',1,'voter_nm5ohbnts',NULL,'2026-07-19 16:47:05.943391','8954a379-9ead-4b55-a163-6ccde7d97198'),('4dc6c7d1-23c4-4566-8f6c-bdbf75d991bf','925bf87f-8b6c-49fb-bcc8-2ea151bf271d',2,'voter_nm5ohbnts',NULL,'2026-07-19 17:23:38.435031','2e3d4968-f9b8-4624-b82c-ac92fd090610'),('4ddba006-5907-4498-bd35-0be54b16179a','f32682c6-ac3e-4b9d-9f7d-cb370948b8a2',0,'voter_nm5ohbnts','web','2026-07-20 10:08:57.214400','8954a379-9ead-4b55-a163-6ccde7d97198'),('98ecfbf6-86b4-4a86-8d11-cd9b302371a5','2eab102a-88fe-48d6-b615-bbf2ec54eaba',1,'voter_nm5ohbnts',NULL,'2026-07-19 19:23:46.082944',NULL),('a40bf603-628a-457b-8caa-86e08726b2d4','2eab102a-88fe-48d6-b615-bbf2ec54eaba',1,'voter_nm5ohbnts',NULL,'2026-07-19 19:23:54.489401','8954a379-9ead-4b55-a163-6ccde7d97198'),('be33dba2-c2f9-4da3-8f58-a1b4d62cf7fb','cad80aae-d344-4e90-8c63-117da6b222c2',1,'voter_nm5ohbnts',NULL,'2026-07-19 19:14:50.333289','8954a379-9ead-4b55-a163-6ccde7d97198'),('c0a81127-2a0c-4766-ad92-090f5cc5c400','80651896-a162-4ce9-bfa0-443f68ee0923',0,'voter_nm5ohbnts',NULL,'2026-07-20 07:07:47.125755','8954a379-9ead-4b55-a163-6ccde7d97198'),('f924dee7-4601-41a1-887d-1eb011670108','eee785d7-0d49-49f7-9612-4d8e97c3805d',4,'voter_nm5ohbnts',NULL,'2026-07-20 10:22:13.552878','8954a379-9ead-4b55-a163-6ccde7d97198'),('f9a94aec-cf38-41ed-9687-f9cb37f43bc0','99e95c1f-1298-449d-8929-332891b1b77b',0,'9a6783fe42dd436396909e4424777721',NULL,'2026-07-19 15:38:41.412898',NULL);
/*!40000 ALTER TABLE `votes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'pollsurveydb'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-07-20 17:38:50
