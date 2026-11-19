-- MySQL dump 10.13  Distrib 26.7.0, for Win64 (x86_64)
--
-- Host: localhost    Database: csi402db
-- ------------------------------------------------------
-- Server version	26.7.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `add_on_services`
--

DROP TABLE IF EXISTS `add_on_services`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `add_on_services` (
  `ServiceId` int NOT NULL AUTO_INCREMENT,
  `ServiceCode` varchar(50) NOT NULL,
  `ServiceName` varchar(150) NOT NULL,
  `PricePerDay` decimal(10,2) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ServiceId`),
  UNIQUE KEY `ServiceCode` (`ServiceCode`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `add_on_services`
--

LOCK TABLES `add_on_services` WRITE;
/*!40000 ALTER TABLE `add_on_services` DISABLE KEYS */;
INSERT INTO `add_on_services` VALUES (1,'INSURANCE','ประกันภัยรถยนต์ชั้น 1',200.00,1),(2,'BABY_SEAT','คาร์ซีทสำหรับเด็ก',100.00,1);
/*!40000 ALTER TABLE `add_on_services` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `bookings`
--

DROP TABLE IF EXISTS `bookings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bookings` (
  `BookingId` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(100) NOT NULL,
  `CarModel` varchar(150) NOT NULL,
  `PickupLocation` varchar(150) NOT NULL,
  `PickupDate` varchar(50) NOT NULL,
  `RentalDays` int NOT NULL,
  `PricePerDay` decimal(10,2) NOT NULL,
  `TotalPrice` decimal(10,2) NOT NULL,
  `BookingDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `HasInsurance` tinyint(1) DEFAULT '0',
  `HasBabySeat` tinyint(1) DEFAULT '0',
  `AddonTotal` decimal(10,2) DEFAULT '0.00',
  `CarId` int DEFAULT NULL,
  `ReturnLocation` varchar(150) DEFAULT NULL,
  `PickupTime` time DEFAULT NULL,
  `ReturnDate` date DEFAULT NULL,
  `ReturnTime` time DEFAULT NULL,
  `DepositAmount` decimal(10,2) NOT NULL DEFAULT '0.00',
  `BookingStatus` varchar(30) NOT NULL DEFAULT 'pending',
  `DiscountAmount` decimal(10,2) NOT NULL DEFAULT '0.00',
  `PromotionCode` varchar(50) DEFAULT NULL,
  `LicenseImagePath` varchar(255) DEFAULT NULL,
  `LicenseStatus` varchar(30) NOT NULL DEFAULT 'pending',
  `LicenseReviewNote` varchar(500) DEFAULT NULL,
  `ReturnMileage` int DEFAULT NULL,
  `ReturnImagePaths` varchar(500) DEFAULT NULL,
  `ReturnSubmittedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`BookingId`),
  KEY `IX_bookings_CarId` (`CarId`),
  CONSTRAINT `FK_bookings_cars_CarId` FOREIGN KEY (`CarId`) REFERENCES `cars` (`CarId`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bookings`
--

LOCK TABLES `bookings` WRITE;
/*!40000 ALTER TABLE `bookings` DISABLE KEYS */;
INSERT INTO `bookings` VALUES (1,'tenz','Toyota Yaris Ativ','สนามบินสุวรรณภูมิ','2569-11-03',5,890.00,4450.00,'2026-11-03 03:56:30',0,0,0.00,1,NULL,NULL,'2569-11-08',NULL,445.00,'cancelled',0.00,NULL,NULL,'pending',NULL,NULL,NULL,NULL),(2,'tine','Toyota Yaris Ativ','สนามบินสุวรรณภูมิ','2569-11-13',3,890.00,2670.00,'2026-11-03 21:18:21',0,0,0.00,1,NULL,NULL,'2569-11-16',NULL,267.00,'cancelled',0.00,NULL,NULL,'pending',NULL,NULL,NULL,NULL),(4,'tenz','tesla Model Y RWD','เชียงใหม่','2569-11-05',1,1800.00,1800.00,'2026-11-05 13:52:53',0,0,0.00,4,NULL,NULL,'2569-11-06',NULL,180.00,'cancelled',0.00,NULL,NULL,'pending',NULL,NULL,NULL,NULL),(5,'tine','tesla Model Y RWD','สนามบินสุวรรณภูมิ','3112-11-15',4,1800.00,6120.00,'2026-11-15 03:06:37',0,0,0.00,4,'สนามบินดอนเมือง','11:00:00','2569-11-19','09:00:00',612.00,'cancelled',1080.00,NULL,'7ada82255e674e51bc91ee39a1c114e6.png','pending',NULL,NULL,NULL,NULL),(6,'tine','Honda Civic e:HEV Rs','สนามบินสุวรรณภูมิ','2026-11-15',1,1111.00,999.90,'2026-11-15 03:50:20',0,0,0.00,5,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-16','09:00:00',99.99,'cancelled',111.10,'K20','81f5d18e1fdb48ffa40d29550c3df288.png','pending',NULL,NULL,NULL,NULL),(7,'tenz','Honda Civic e:HEV Rs','สนามบินสุวรรณภูมิ','2026-11-16',3,1111.00,3333.00,'2026-11-16 01:07:52',0,0,0.00,5,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-19','09:00:00',333.30,'cancelled',0.00,NULL,'8c069833c30444fb9ba550ffe1ddd471.png','approved',NULL,NULL,NULL,NULL),(8,'tine','tesla Model Y RWD','สนามบินสุวรรณภูมิ','2026-11-18',3,1800.00,4860.00,'2026-11-18 16:44:55',0,0,0.00,4,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-21','03:03:00',486.00,'cancelled',540.00,'K20','86a9464e6ef8449584b26e5cf7032278.png','pending',NULL,NULL,NULL,NULL),(9,'tine','Toyota Yaris Ativ','สนามบินสุวรรณภูมิ','2026-11-18',3,890.00,2403.00,'2026-11-18 16:57:22',0,0,0.00,1,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-21','00:00:00',240.30,'cancelled',267.00,'K20','6f1a436fe9d14ab689c354036fe97875.png','pending',NULL,NULL,NULL,NULL),(10,'tine','tesla Model Y RWD','สนามบินสุวรรณภูมิ','2026-11-18',1,1800.00,1620.00,'2026-11-18 17:00:24',0,0,0.00,4,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-19','09:00:00',162.00,'cancelled',180.00,'K20','9283fa9f34df4fca9c1df304dc273dd3.png','pending',NULL,NULL,NULL,NULL),(11,'tine','Toyota Yaris Ativ','สนามบินสุวรรณภูมิ','2026-11-18',1,890.00,801.00,'2026-11-18 17:05:29',0,0,0.00,1,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-19','09:00:00',80.10,'cancelled',89.00,'K20','bb978ba4f62343729c315f9de557c5bb.png','pending',NULL,NULL,NULL,NULL),(12,'tenz','Honda Civic e:HEV Rs','สนามบินสุวรรณภูมิ','2026-11-19',1,1111.00,1111.00,'2026-11-19 00:21:59',0,0,0.00,5,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-20','09:00:00',111.10,'cancelled',0.00,NULL,'6dce547c06924649b2e8daafc8765250.png','approved',NULL,NULL,NULL,NULL),(13,'tenz','tesla Model Y RWD','สนามบินดอนเมือง','2026-11-19',2,1800.00,2520.00,'2026-11-19 00:43:03',0,0,0.00,4,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-21','00:00:00',252.00,'completed',1080.00,'T122222','de2d3230a67e46bc821a21584dbc04d2.png','approved',NULL,NULL,NULL,NULL),(14,'senn','Honda Civic e:HEV Rs','สนามบินสุวรรณภูมิ','2026-11-19',2,1111.00,1555.40,'2026-11-19 01:30:04',0,0,0.00,5,'สนามบินดอนเมือง','09:00:00','2026-11-21','09:00:00',155.54,'completed',666.60,'T122222','83d83fadd7a24ed9970be8cb76e3f9a7.jpg','approved',NULL,40000,'e1bbeee732c74b28ae3e43479c89731d.jpg','2026-11-19 02:17:41'),(15,'senn','Toyota Corolla Altis 2023','สนามบินดอนเมือง','2026-11-19',2,1090.00,1526.00,'2026-11-19 19:54:38',0,0,0.00,7,'สนามบินสุวรรณภูมิ','09:00:00','2026-11-21','09:00:00',152.60,'confirmed',654.00,'T122222','7b232d4b6a7e4f5cb10634ab0c912745.jpg','approved',NULL,NULL,NULL,NULL);
/*!40000 ALTER TABLE `bookings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `car_inspections`
--

DROP TABLE IF EXISTS `car_inspections`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `car_inspections` (
  `InspectionId` int NOT NULL AUTO_INCREMENT,
  `BookingId` int NOT NULL,
  `CarId` int NOT NULL,
  `InspectionStage` varchar(30) NOT NULL,
  `Mileage` int NOT NULL,
  `FuelLevel` int NOT NULL,
  `ConditionNotes` varchar(2000) DEFAULT NULL,
  `DamageAmount` decimal(10,2) NOT NULL DEFAULT '0.00',
  `InspectionStatus` varchar(30) NOT NULL DEFAULT 'complete',
  `InspectedBy` varchar(100) NOT NULL,
  `InspectedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InspectionId`),
  KEY `IX_inspections_BookingId` (`BookingId`),
  KEY `FK_inspections_cars_CarId` (`CarId`),
  CONSTRAINT `FK_inspections_bookings_BookingId` FOREIGN KEY (`BookingId`) REFERENCES `bookings` (`BookingId`) ON DELETE RESTRICT,
  CONSTRAINT `FK_inspections_cars_CarId` FOREIGN KEY (`CarId`) REFERENCES `cars` (`CarId`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `car_inspections`
--

LOCK TABLES `car_inspections` WRITE;
/*!40000 ALTER TABLE `car_inspections` DISABLE KEYS */;
INSERT INTO `car_inspections` VALUES (1,13,4,'pre_release',10000,0,'0',0.00,'passed','bolt','2026-11-19 00:59:56'),(2,13,4,'before_pickup',10000,0,'0',0.00,'complete','bolt','2026-11-19 01:04:26'),(3,13,4,'after_return',10700,0,'0',0.00,'complete','test_employee_20261118','2026-11-19 01:11:43'),(4,14,5,'pre_release',37000,60,NULL,0.00,'passed','test_service_20261118','2026-11-19 01:39:34'),(5,14,5,'before_pickup',37000,60,'0',0.00,'complete','bolt','2026-11-19 01:42:47'),(6,14,5,'return_received',40000,10,'0',0.00,'complete','bolt','2026-11-19 02:18:47'),(7,14,5,'after_return',40000,10,'ไม่มี',0.00,'passed','bolt','2026-11-19 02:19:05');
/*!40000 ALTER TABLE `car_inspections` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cars`
--

DROP TABLE IF EXISTS `cars`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cars` (
  `CarId` int NOT NULL AUTO_INCREMENT,
  `CarName` varchar(150) NOT NULL,
  `PricePerDay` decimal(10,2) NOT NULL,
  `ImageUrl` varchar(255) NOT NULL,
  `Status` varchar(50) DEFAULT 'ว่าง',
  `Details` varchar(255) NOT NULL,
  `TransmissionType` varchar(20) DEFAULT NULL,
  `Seats` int DEFAULT NULL,
  `Location` varchar(150) DEFAULT NULL,
  PRIMARY KEY (`CarId`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cars`
--

LOCK TABLES `cars` WRITE;
/*!40000 ALTER TABLE `cars` DISABLE KEYS */;
INSERT INTO `cars` VALUES (1,'Toyota Yaris Ativ',890.00,'https://cdn.carsome.co.th/news/ativ_exterior1_13_11zon.jpg','ว่าง','4 ที่นั่ง | อัตโนมัติ | 1.2L','automatic',4,'สนามบินสุวรรณภูมิ'),(2,'Honda Civic FK RS',1590.00,'https://img.autofun.co.th/file/014b1e46df4a41b0a81fc2378f58af5e.jpg','ว่าง','5 ที่นั่ง | อัตโนมัติ | 1.5L Turbo','automatic',5,'สนามบินดอนเมือง'),(3,'Toyota Fortuner GR',2500.00,'https://static.thairath.co.th/media/B6FtNKtgSqRqbnNsUi2v7X1POYnmGKY8lBBRBuKKc7CIVMCW2CrrxIZsstH2VHY8CuWZE.jpg','ว่าง','7 ที่นั่ง | อัตโนมัติ | SUV ลุยครอบครัว','automatic',7,'เชียงใหม่'),(4,'tesla Model Y RWD',1800.00,'https://upload.wikimedia.org/wikipedia/commons/e/e7/Tesla_Model_Y_Premium_%28Facelift%29_%E2%80%93_f_05052026.jpg?utm_source=en.wikipedia.org&utm_campaign=index&utm_content=original','ว่าง','รถยนต์ไฟฟ้า 100% (EV) ประเภท SUV 5 ที่นั่ง ขับเคลื่อนล้อหลัง (RWD) ระบบเกียร์อัตโนมัติ','automatic',5,'ภูเก็ต'),(5,'Honda Civic e:HEV Rs',1111.00,'https://imgcdn.zigwheels.co.th/large/gallery/exterior/9/3161/honda-civic-73234.jpg','ว่าง','เครื่องยนต์. เบนซิน 4 สูบ DOHC 16 วาล์ว ขนาด 2.0 ลิตร (1,993 ซีซี) กำลัง 141 แรงม้า ทำงานร่วมกับมอเตอร์ไฟฟ้า 2 ตัว','automatic',4,'สนามบินสุวรรณภูมิ'),(6,'Honda City 2023',990.00,'https://images.openai.com/static-rsc-4/zcLAo7Uv7ItpU4vlbzDlEALhyiaE9TYMDr9Kf7bSnMUeSfLGdgFM9Dp_t68a7FkNrfOe-ukCaiO6ISWCfEC16IW8DV8zEoBmxj5k9y0WOH4PRTJkOHboD4-bxY3jsQql0gdws2oz5QKcqLiL22EmrobTZjxy7XQrloCs5XXmtyaICwYLB5H_MdUt0HCAGLo2?purpose=fullsize','ว่าง','5 ที่นั่ง | เกียร์ CVT | 1.0L Turbo | เบนซิน','automatic',5,'ภูเก็ต'),(7,'Toyota Corolla Altis 2023',1090.00,'https://images.openai.com/static-rsc-4/O-sVJlN_vZJBSPvogeNGnqdTZXHO-1k7DHjHCG-2Mg6FD2ebvz_WWJKa7RTT8jz1m-HEBIgMaf63fLOu2Eip0UyrYFp6aqXtddaZPGAbeRvK9DxG0Hni5WeYqfzccgjze6gOHpv8prnbqutIypLzni217pTDLAtHY-fmC5H3GNa_tCRjRUtnayVylje5nEjw?purpose=fullsize','ว่าง','5 ที่นั่ง | CVT | 1.6L | เบนซิน','automatic',5,'สนามบินดอนเมือง'),(8,'Mitsubishi Xpander 2023',1290.00,'https://images.openai.com/static-rsc-4/kmQ6qlI77Ya30VcDdM7K5H9B07x414xkJsy1KBpKzQovI7xFcAtSMI9gTVTBzf5sCIJ8jQEgYXtB7jXBAakdQq3sGrLs8T8nSKYAR2AN01AWZE2WtenK4w9HV6fI6ukXJ1_YhjHDkTSs8gcN8WxYg1Jmf0AQ-6lAkYJ8JkxMdZP6vkCYWeEhg83kIPtsQO7V?purpose=fullsize','ว่าง','7 ที่นั่ง | อัตโนมัติ | 1.5L | เบนซิน','automatic',7,'ภูเก็ต');
/*!40000 ALTER TABLE `cars` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `login_history`
--

DROP TABLE IF EXISTS `login_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `login_history` (
  `LoginHistoryID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `Username` varchar(50) NOT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `LoggedInAtUTC` datetime(6) NOT NULL,
  PRIMARY KEY (`LoginHistoryID`)
) ENGINE=InnoDB AUTO_INCREMENT=144 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `login_history`
--

LOCK TABLES `login_history` WRITE;
/*!40000 ALTER TABLE `login_history` DISABLE KEYS */;
INSERT INTO `login_history` VALUES (1,10,'shoppeefood','warat.chu@spumail.net','2026-10-15 23:50:05.701301'),(2,9,'tine22','tinexx59@gmail.com','2026-10-15 23:50:59.600722'),(3,8,'tine','warattine@gmail.com','2026-10-15 23:55:13.520531'),(4,11,'bolt','papapa59005@gmail.com','2026-10-15 23:59:25.999995'),(5,11,'bolt','papapa59005@gmail.com','2026-10-16 00:16:30.918527'),(6,12,'tenz','warat.chu@spumail.net','2026-11-03 00:44:42.861958'),(7,12,'tenz','warat.chu@spumail.net','2026-11-03 02:07:13.140451'),(8,12,'tenz','warat.chu@spumail.net','2026-11-03 02:11:01.283954'),(9,12,'tenz','warat.chu@spumail.net','2026-11-03 02:43:24.981385'),(10,12,'tenz','warat.chu@spumail.net','2026-11-03 02:45:52.910140'),(11,12,'tenz','warat.chu@spumail.net','2026-11-03 02:49:14.786401'),(12,12,'tenz','warat.chu@spumail.net','2026-11-03 03:06:05.271642'),(13,12,'tenz','warat.chu@spumail.net','2026-11-03 03:06:24.053561'),(14,12,'tenz','warat.chu@spumail.net','2026-11-03 03:08:46.340770'),(15,8,'tine','warattine@gmail.com','2026-11-03 21:17:32.051593'),(16,11,'bolt','papapa59005@gmail.com','2026-11-04 23:12:13.833981'),(17,11,'bolt','papapa59005@gmail.com','2026-11-04 23:46:30.521598'),(18,11,'bolt','papapa59005@gmail.com','2026-11-04 23:52:39.800155'),(19,11,'bolt','papapa59005@gmail.com','2026-11-04 23:54:55.302167'),(20,12,'tenz','warat.chu@spumail.net','2026-11-05 00:00:05.830160'),(21,11,'bolt','papapa59005@gmail.com','2026-11-05 00:00:17.076097'),(22,11,'bolt','papapa59005@gmail.com','2026-11-05 00:05:26.988459'),(23,12,'tenz','warat.chu@spumail.net','2026-11-05 00:09:23.909545'),(24,11,'bolt','papapa59005@gmail.com','2026-11-05 00:12:44.261154'),(25,11,'bolt','papapa59005@gmail.com','2026-11-05 00:13:47.275480'),(26,11,'bolt','papapa59005@gmail.com','2026-11-05 00:15:24.060168'),(27,11,'bolt','papapa59005@gmail.com','2026-11-05 00:25:59.746908'),(28,11,'bolt','papapa59005@gmail.com','2026-11-05 00:29:40.972277'),(29,11,'bolt','papapa59005@gmail.com','2026-11-05 00:31:57.457788'),(30,11,'bolt','papapa59005@gmail.com','2026-11-05 00:34:03.767717'),(31,11,'bolt','papapa59005@gmail.com','2026-11-05 00:42:56.528476'),(32,12,'tenz','warat.chu@spumail.net','2026-11-05 13:52:05.236365'),(33,11,'bolt','papapa59005@gmail.com','2026-11-05 20:57:46.147864'),(34,11,'bolt','papapa59005@gmail.com','2026-11-05 21:21:12.103603'),(35,11,'bolt','papapa59005@gmail.com','2026-11-05 21:35:52.241836'),(36,11,'bolt','papapa59005@gmail.com','2026-11-10 19:37:21.252605'),(37,11,'bolt','papapa59005@gmail.com','2026-11-12 01:00:07.164083'),(38,11,'bolt','papapa59005@gmail.com','2026-11-12 01:00:43.090072'),(39,12,'tenz','warat.chu@spumail.net','2026-11-12 01:01:54.868759'),(40,11,'bolt','papapa59005@gmail.com','2026-11-12 01:02:52.104990'),(41,11,'bolt','papapa59005@gmail.com','2026-11-12 01:57:04.669259'),(42,11,'bolt','papapa59005@gmail.com','2026-11-12 01:57:44.089850'),(43,11,'bolt','papapa59005@gmail.com','2026-11-12 01:58:12.289142'),(44,11,'bolt','papapa59005@gmail.com','2026-11-12 02:02:09.369161'),(45,12,'tenz','warat.chu@spumail.net','2026-11-12 02:05:37.239029'),(46,11,'bolt','papapa59005@gmail.com','2026-11-12 02:05:42.721659'),(47,11,'bolt','papapa59005@gmail.com','2026-11-12 17:15:58.761017'),(48,11,'bolt','papapa59005@gmail.com','2026-11-15 01:35:09.465100'),(49,8,'tine','warattine@gmail.com','2026-11-15 03:05:38.184757'),(50,11,'bolt','papapa59005@gmail.com','2026-11-15 03:07:02.601657'),(51,11,'bolt','papapa59005@gmail.com','2026-11-15 03:10:48.452522'),(52,8,'tine','warattine@gmail.com','2026-11-15 03:17:56.770253'),(53,8,'tine','warattine@gmail.com','2026-11-15 03:19:15.959991'),(54,11,'bolt','papapa59005@gmail.com','2026-11-15 03:19:21.501037'),(55,8,'tine','warattine@gmail.com','2026-11-15 03:26:49.779013'),(56,11,'bolt','papapa59005@gmail.com','2026-11-15 03:30:48.073782'),(57,11,'bolt','papapa59005@gmail.com','2026-11-15 03:31:32.196145'),(58,11,'bolt','papapa59005@gmail.com','2026-11-15 03:36:08.361003'),(59,8,'tine','warattine@gmail.com','2026-11-15 03:36:25.031338'),(60,11,'bolt','papapa59005@gmail.com','2026-11-15 03:36:51.481182'),(61,8,'tine','warattine@gmail.com','2026-11-15 03:38:11.820475'),(62,11,'bolt','papapa59005@gmail.com','2026-11-15 03:42:03.635951'),(63,8,'tine','warattine@gmail.com','2026-11-15 03:43:02.824473'),(64,12,'tenz','warat.chu@spumail.net','2026-11-16 01:05:58.850705'),(65,11,'bolt','papapa59005@gmail.com','2026-11-16 01:06:42.115185'),(66,12,'tenz','warat.chu@spumail.net','2026-11-16 01:07:25.540132'),(67,11,'bolt','papapa59005@gmail.com','2026-11-16 01:08:12.641853'),(68,12,'tenz','warat.chu@spumail.net','2026-11-16 01:08:57.169709'),(69,11,'bolt','papapa59005@gmail.com','2026-11-16 01:11:30.500566'),(70,12,'tenz','warat.chu@spumail.net','2026-11-16 01:11:53.292152'),(71,11,'bolt','papapa59005@gmail.com','2026-11-16 01:12:33.495983'),(72,12,'tenz','warat.chu@spumail.net','2026-11-16 01:13:08.059004'),(73,11,'bolt','papapa59005@gmail.com','2026-11-16 01:21:48.095118'),(74,12,'tenz','warat.chu@spumail.net','2026-11-16 01:23:39.619469'),(75,11,'bolt','papapa59005@gmail.com','2026-11-16 01:36:15.310859'),(76,11,'bolt','papapa59005@gmail.com','2026-11-16 01:43:59.734763'),(77,11,'bolt','papapa59005@gmail.com','2026-11-17 01:57:13.615181'),(78,12,'tenz','warat.chu@spumail.net','2026-11-17 02:06:48.125899'),(79,11,'bolt','papapa59005@gmail.com','2026-11-18 02:52:16.275585'),(80,14,'test_employee_20261118','test_employee_20261118@driverrent.local','2026-11-18 02:53:05.189202'),(81,15,'test_manager_20261118','test_manager_20261118@driverrent.local','2026-11-18 02:53:27.435161'),(82,16,'test_finance_20261118','test_finance_20261118@driverrent.local','2026-11-18 02:53:53.418908'),(83,17,'test_service_20261118','test_service_20261118@driverrent.local','2026-11-18 02:54:09.400558'),(84,12,'tenz','warat.chu@spumail.net','2026-11-18 02:56:18.106643'),(85,11,'bolt','papapa59005@gmail.com','2026-11-18 02:58:24.202635'),(86,12,'tenz','warat.chu@spumail.net','2026-11-18 02:58:51.257523'),(87,11,'bolt','papapa59005@gmail.com','2026-11-18 02:59:09.642285'),(88,8,'tine','warattine@gmail.com','2026-11-18 16:42:48.138504'),(89,11,'bolt','papapa59005@gmail.com','2026-11-18 17:05:51.033178'),(90,12,'tenz','warat.chu@spumail.net','2026-11-19 00:21:41.068557'),(91,11,'bolt','papapa59005@gmail.com','2026-11-19 00:26:32.244671'),(92,12,'tenz','warat.chu@spumail.net','2026-11-19 00:26:59.568881'),(93,11,'bolt','papapa59005@gmail.com','2026-11-19 00:27:38.083817'),(94,12,'tenz','warat.chu@spumail.net','2026-11-19 00:28:24.423717'),(95,11,'bolt','papapa59005@gmail.com','2026-11-19 00:32:36.459738'),(96,11,'bolt','papapa59005@gmail.com','2026-11-19 00:36:00.481703'),(97,12,'tenz','warat.chu@spumail.net','2026-11-19 00:37:38.291513'),(98,11,'bolt','papapa59005@gmail.com','2026-11-19 00:38:38.735342'),(99,12,'tenz','warat.chu@spumail.net','2026-11-19 00:41:19.537074'),(100,11,'bolt','papapa59005@gmail.com','2026-11-19 00:43:31.865255'),(101,11,'bolt','papapa59005@gmail.com','2026-11-19 00:47:34.739049'),(102,16,'test_finance_20261118','test_finance_20261118@driverrent.local','2026-11-19 00:48:35.991545'),(103,12,'tenz','warat.chu@spumail.net','2026-11-19 00:49:04.886775'),(104,16,'test_finance_20261118','test_finance_20261118@driverrent.local','2026-11-19 00:49:31.856950'),(105,12,'tenz','warat.chu@spumail.net','2026-11-19 00:49:53.927830'),(106,12,'tenz','warat.chu@spumail.net','2026-11-19 00:53:54.742037'),(107,11,'bolt','papapa59005@gmail.com','2026-11-19 00:55:54.827595'),(108,14,'test_employee_20261118','test_employee_20261118@driverrent.local','2026-11-19 00:56:06.974126'),(109,11,'bolt','papapa59005@gmail.com','2026-11-19 00:56:32.611672'),(110,14,'test_employee_20261118','test_employee_20261118@driverrent.local','2026-11-19 01:00:50.617356'),(111,11,'bolt','papapa59005@gmail.com','2026-11-19 01:01:21.631676'),(112,12,'tenz','warat.chu@spumail.net','2026-11-19 01:05:28.352023'),(113,14,'test_employee_20261118','test_employee_20261118@driverrent.local','2026-11-19 01:11:07.750942'),(114,16,'test_finance_20261118','test_finance_20261118@driverrent.local','2026-11-19 01:11:57.006938'),(115,12,'tenz','warat.chu@spumail.net','2026-11-19 01:16:55.428641'),(116,16,'test_finance_20261118','test_finance_20261118@driverrent.local','2026-11-19 01:18:25.723656'),(117,13,'senn','tinexx59@gmail.com','2026-11-19 01:28:23.237977'),(118,11,'bolt','papapa59005@gmail.com','2026-11-19 01:32:25.520648'),(119,13,'senn','tinexx59@gmail.com','2026-11-19 01:33:42.743217'),(120,16,'test_finance_20261118','test_finance_20261118@driverrent.local','2026-11-19 01:34:00.402344'),(121,13,'senn','tinexx59@gmail.com','2026-11-19 01:36:31.438854'),(122,14,'test_employee_20261118','test_employee_20261118@driverrent.local','2026-11-19 01:37:51.099908'),(123,17,'test_service_20261118','test_service_20261118@driverrent.local','2026-11-19 01:39:05.872125'),(124,14,'test_employee_20261118','test_employee_20261118@driverrent.local','2026-11-19 01:40:03.525173'),(125,11,'bolt','papapa59005@gmail.com','2026-11-19 01:41:53.812626'),(126,13,'senn','tinexx59@gmail.com','2026-11-19 01:43:03.171262'),(127,11,'bolt','papapa59005@gmail.com','2026-11-19 02:16:56.250398'),(128,13,'senn','tinexx59@gmail.com','2026-11-19 02:17:28.244218'),(129,11,'bolt','papapa59005@gmail.com','2026-11-19 02:17:49.028710'),(130,11,'bolt','papapa59005@gmail.com','2026-11-19 02:27:33.824229'),(131,11,'bolt','papapa59005@gmail.com','2026-11-19 19:02:42.696109'),(132,13,'senn','tinexx59@gmail.com','2026-11-19 19:54:17.490575'),(133,11,'bolt','papapa59005@gmail.com','2026-11-19 19:55:21.766340'),(134,13,'senn','tinexx59@gmail.com','2026-11-19 19:55:38.464910'),(135,11,'bolt','papapa59005@gmail.com','2026-11-19 19:56:17.223671'),(136,13,'senn','tinexx59@gmail.com','2026-11-19 19:56:48.453078'),(137,11,'bolt','papapa59005@gmail.com','2026-11-19 19:57:27.371079'),(138,13,'senn','tinexx59@gmail.com','2026-11-19 20:04:45.133474'),(139,11,'bolt','papapa59005@gmail.com','2026-11-19 20:08:35.539794'),(140,13,'senn','tinexx59@gmail.com','2026-11-19 20:09:10.344971'),(141,13,'senn','tinexx59@gmail.com','2026-11-19 20:09:20.672330'),(142,11,'bolt','papapa59005@gmail.com','2026-11-19 20:09:26.264889'),(143,13,'senn','tinexx59@gmail.com','2026-11-19 22:58:46.866980');
/*!40000 ALTER TABLE `login_history` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `maintenance_records`
--

DROP TABLE IF EXISTS `maintenance_records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `maintenance_records` (
  `MaintenanceId` int NOT NULL AUTO_INCREMENT,
  `CarId` int NOT NULL,
  `BookingId` int DEFAULT NULL,
  `IssueDescription` varchar(2000) NOT NULL,
  `RepairCost` decimal(10,2) NOT NULL DEFAULT '0.00',
  `MaintenanceStatus` varchar(30) NOT NULL DEFAULT 'reported',
  `ReportedBy` varchar(100) NOT NULL,
  `ResolvedBy` varchar(100) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ResolvedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`MaintenanceId`),
  KEY `IX_maintenance_CarId` (`CarId`),
  KEY `FK_maintenance_bookings_BookingId` (`BookingId`),
  CONSTRAINT `FK_maintenance_bookings_BookingId` FOREIGN KEY (`BookingId`) REFERENCES `bookings` (`BookingId`) ON DELETE SET NULL,
  CONSTRAINT `FK_maintenance_cars_CarId` FOREIGN KEY (`CarId`) REFERENCES `cars` (`CarId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `maintenance_records`
--

LOCK TABLES `maintenance_records` WRITE;
/*!40000 ALTER TABLE `maintenance_records` DISABLE KEYS */;
/*!40000 ALTER TABLE `maintenance_records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payments`
--

DROP TABLE IF EXISTS `payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payments` (
  `PaymentId` int NOT NULL AUTO_INCREMENT,
  `BookingId` int NOT NULL,
  `PaymentType` varchar(30) NOT NULL,
  `Amount` decimal(10,2) NOT NULL,
  `PaymentStatus` varchar(30) NOT NULL DEFAULT 'pending',
  `ProofFileName` varchar(255) DEFAULT NULL,
  `ReferenceNumber` varchar(100) DEFAULT NULL,
  `Note` varchar(500) DEFAULT NULL,
  `ReviewedBy` varchar(100) DEFAULT NULL,
  `ReviewedAt` datetime DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`PaymentId`),
  KEY `IX_payments_BookingId` (`BookingId`),
  CONSTRAINT `FK_payments_bookings_BookingId` FOREIGN KEY (`BookingId`) REFERENCES `bookings` (`BookingId`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payments`
--

LOCK TABLES `payments` WRITE;
/*!40000 ALTER TABLE `payments` DISABLE KEYS */;
INSERT INTO `payments` VALUES (1,7,'deposit',333.30,'verified','2d63ca50f5ea430a97b7fadf7e72f760.png',NULL,NULL,'bolt','2026-11-16 01:11:43','2026-11-16 01:11:20'),(2,7,'rental',3333.00,'verified','ab9990aafb6a4699a231a565eed2c637.png',NULL,NULL,'bolt','2026-11-16 01:12:57','2026-11-16 01:12:19'),(3,12,'deposit',111.10,'verified','c8ac34c6ed834ccc979f2b12926d9113.png',NULL,NULL,'bolt','2026-11-19 00:26:51','2026-11-19 00:22:12'),(4,12,'rental',1111.00,'verified','bfdd4edd0e264bacb8ecb97553a81baa.png',NULL,NULL,'bolt','2026-11-19 00:28:17','2026-11-19 00:27:26'),(5,12,'refund',1222.10,'pending',NULL,NULL,'คืนเงินเต็มจำนวนจากการยกเลิกการจองโดยผู้ดูแลระบบ',NULL,NULL,'2026-11-19 00:38:47'),(6,7,'refund',3666.30,'pending',NULL,NULL,'คืนเงินเต็มจำนวนจากการยกเลิกการจองโดยผู้ดูแลระบบ',NULL,NULL,'2026-11-19 00:38:49'),(7,13,'deposit',252.00,'verified','dbf297ae36e540b9861c8ac6f00c80ca.png',NULL,NULL,'test_finance_20261118','2026-11-19 00:48:49','2026-11-19 00:43:12'),(8,13,'rental',2520.00,'verified','2798bef5e8c94e46a196d3b7b1c1381f.png','111',NULL,'test_finance_20261118','2026-11-19 00:49:37','2026-11-19 00:49:18'),(9,13,'refund',252.00,'completed',NULL,'#13','คืนมัดจำเต็มจำนวน','test_finance_20261118','2026-11-19 01:16:07','2026-11-19 01:16:07'),(10,14,'deposit',155.54,'verified','d4114c1ac6ce406595598d59e75ab256.jpg',NULL,NULL,'test_finance_20261118','2026-11-19 01:34:35','2026-11-19 01:33:50'),(11,14,'rental',1555.40,'verified','5042ef08fb344dcdb95e9a6469d8cbbc.jpg',NULL,NULL,'bolt','2026-11-19 01:42:06','2026-11-19 01:36:46'),(12,15,'deposit',152.60,'verified','93429991e9404662ada5edd3226c6a38.jpg',NULL,NULL,'bolt','2026-11-19 19:55:30','2026-11-19 19:55:12'),(13,15,'rental',1526.00,'verified','b3b16627222c4a1e965a3c719a05949b.jpg',NULL,NULL,'bolt','2026-11-19 19:56:27','2026-11-19 19:56:05');
/*!40000 ALTER TABLE `payments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `promotions`
--

DROP TABLE IF EXISTS `promotions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promotions` (
  `PromotionId` int NOT NULL AUTO_INCREMENT,
  `PromotionName` varchar(150) NOT NULL,
  `PromotionCode` varchar(50) DEFAULT NULL,
  `ConditionType` varchar(30) NOT NULL,
  `DiscountPercent` decimal(5,2) NOT NULL,
  `StartDate` date DEFAULT NULL,
  `EndDate` date DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`PromotionId`),
  UNIQUE KEY `PromotionCode` (`PromotionCode`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `promotions`
--

LOCK TABLES `promotions` WRITE;
/*!40000 ALTER TABLE `promotions` DISABLE KEYS */;
INSERT INTO `promotions` VALUES (1,'ส่วนลดลูกค้าใหม่','T122222','custom',30.00,'2026-11-15','2026-11-29',1,'2026-11-12 01:40:21'),(2,'เช่าครบ 3 วัน',NULL,'3_days',15.00,NULL,NULL,0,'2026-11-12 01:40:21'),(3,'เช่าระยะยาวครบ 7 วัน',NULL,'7_days',20.00,NULL,NULL,0,'2026-11-12 01:40:21'),(4,'โปรโมชั่นวันจันทร์ถึงพฤหัสบดี',NULL,'weekday',10.00,NULL,NULL,0,'2026-11-12 01:40:21'),(5,'ส่วนลดลูกค้า','K20','custom',10.00,'2026-11-15','2026-11-20',1,'2026-11-15 03:37:52');
/*!40000 ALTER TABLE `promotions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `RoleId` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(50) NOT NULL,
  `Description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`RoleId`),
  UNIQUE KEY `RoleName` (`RoleName`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'Customer','ลูกค้า'),(2,'Employee','พนักงาน'),(3,'Manager','ผู้จัดการ'),(4,'Finance','ฝ่ายการเงิน'),(5,'Service','ทีมเซอร์วิส'),(6,'Admin','ผู้ดูแลระบบ');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `system_settings`
--

DROP TABLE IF EXISTS `system_settings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `system_settings` (
  `SettingKey` varchar(100) NOT NULL,
  `SettingValue` varchar(500) NOT NULL,
  PRIMARY KEY (`SettingKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `system_settings`
--

LOCK TABLES `system_settings` WRITE;
/*!40000 ALTER TABLE `system_settings` DISABLE KEYS */;
INSERT INTO `system_settings` VALUES ('BankAccount','0912489817'),('BankAccountName','วรัท ชื่นไชยชาญ'),('BankName','กสิกร'),('PromptPayId','0992439640');
/*!40000 ALTER TABLE `system_settings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usermodels`
--

DROP TABLE IF EXISTS `usermodels`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usermodels` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(255) DEFAULT NULL,
  `Password` varchar(255) DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `Phone` varchar(50) DEFAULT NULL,
  `Gender` varchar(50) DEFAULT NULL,
  `Role` varchar(50) DEFAULT 'User',
  `RegisterDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `IdCard` varchar(13) DEFAULT NULL,
  `DriverLicense` varchar(20) DEFAULT NULL,
  `DriverLicenseImagePath` varchar(255) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usermodels`
--

LOCK TABLES `usermodels` WRITE;
/*!40000 ALTER TABLE `usermodels` DISABLE KEYS */;
INSERT INTO `usermodels` VALUES (8,'tine','AQAAAAIAAYagAAAAEBRIiXYxpw1W5DGW/i/2v68ThaWmlGr6qumQISX9H3Wh3GCIixIeZB2cVdj27avBXw==','warattine@gmail.com','0999999999','ชาย','Customer','2026-10-15 22:13:48',NULL,NULL,NULL,1),(11,'bolt','AQAAAAIAAYagAAAAEJw3QKTDHvqhwFW8XRuVF2BKvaQ8HFnYLfUWPJl4iIpPHeEQ4SitS8C1/7dvKq+mAw==','papapa59005@gmail.com','1111111111','หญิง','Admin','2026-10-15 23:58:57',NULL,NULL,NULL,1),(12,'tenz','AQAAAAIAAYagAAAAEIj1qkG8BWxuQ46odbaZnN2PjLt+n1l71WZzcDEB/K8X+ismyjbth6Y9mh4XKDxkIw==','warat.chu@spumail.net','2222222222','ชาย','Customer','2026-11-03 00:44:14',NULL,NULL,NULL,1),(13,'senn','AQAAAAIAAYagAAAAEK8vBGSShU1WKkbuYpjJJbC+PJY/ZAGQIBTf4GR2YvbopS3r3EC8SS8oP5zpkaFqTg==','tinexx59@gmail.com','1111111111','หญิง','Customer','2026-11-03 02:15:53',NULL,NULL,NULL,1),(14,'test_employee_20261118','AQAAAAIAAYagAAAAEF6D4vbmqbPQnM07/NyPndVaaYEkyZ9H6YtNVhvxefbCkQwj+twcnDwvTJJKidv8VQ==','test_employee_20261118@driverrent.local','0000000000','ไม่ระบุ','Employee','2026-11-18 02:50:42',NULL,NULL,NULL,1),(15,'test_manager_20261118','AQAAAAIAAYagAAAAEJmA/Z3XwGZh2h0ZUwq9GuLceS6PqACy4vCrZjLaMkSmulQPfhekd0G3/QCEXLGTcw==','test_manager_20261118@driverrent.local','0000000000','ไม่ระบุ','Manager','2026-11-18 02:50:42',NULL,NULL,NULL,1),(16,'test_finance_20261118','AQAAAAIAAYagAAAAEJVehvNckUlkHjA/dDUudJBA3FlYn+x/dWEzSE1PKIirojwKqFOyjFqd+RTvuzAg/g==','test_finance_20261118@driverrent.local','0000000000','ไม่ระบุ','Finance','2026-11-18 02:50:42',NULL,NULL,NULL,1),(17,'test_service_20261118','AQAAAAIAAYagAAAAECGy0ES2AzyAxlOdy+6AAOUhT29zlDQYJTVBuiQGrLsqTdiIICkNNDU7NL1DAXy04A==','test_service_20261118@driverrent.local','0000000000','ไม่ระบุ','Service','2026-11-18 02:50:42',NULL,NULL,NULL,1);
/*!40000 ALTER TABLE `usermodels` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-20  1:53:36
