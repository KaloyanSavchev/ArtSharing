# ArtSharing 🎨

ArtSharing е уеб платформа, насочена към артисти, които искат да споделят своите произведения на изкуството в приятелска и фокусирана среда, създадена специално за тази цел. Сайтът предлага изчистен и елегантен дизайн, който поставя съдържанието на преден план и елиминира всичко излишно, за да не разсейва потребителите.

## 🔧 Основни функционалности

- 📸 Качване на постове със снимки и описание
- ❤️ Харесване на постове
- 💬 Коментари и отговори към коментари (вкл. редакция и изтриване)
- 📌 Категории на публикации
- 👥 Възможност за следване на други потребители
- 📄 Собствен профил с информация, аватар и галерия
- 🔔 Сигнализиране на неподходящо съдържание (пост, коментар, потребител)
- 📝 Система за обратна връзка към администраторите
- 🌓 Превключване между тъмна и светла тема
- 🧑‍💼 Админ панел за управление на потребители, категории и сигнали

## 🛠️ Използвани технологии

- **ASP.NET Core MVC** – за изграждане на уеб приложението
- **Entity Framework Core (MySQL)** – ORM за достъп до базата данни
- **ASP.NET Identity** – система за удостоверяване и авторизация на потребители
- **Bootstrap 5** – за отзивчив и модерен дизайн
- **jQuery & Vanilla JavaScript** – за динамична работа на интерфейса (лайкове, зареждане на коментари и др.)
- **Faker.NET** – за генериране на тестови данни в unit тестовете
- **xUnit / NUnit** – за unit тестове на service слой
- **Visual Studio** + **Rider** – използвани при разработка

## 🚀 Как да стартирате проекта локално

1. **Клонирайте хранилището**
   ```bash
   git clone https://github.com/your-username/ArtSharing.git
   ```

2. **Отворете проекта във Visual Studio 2022+**

3. **Настройте базата данни**:
   - Уверете се, че имате работещ MySQL сървър.
   - Конфигурирайте connection string в `appsettings.json`:
     ```json
     "ConnectionStrings": {
         "DefaultConnection": "Server=localhost;Database=ArtSharingDb;User=root;Password=yourpassword;"
     }
     ```

4. **Изпълнете миграциите и стартирайте**
   - Отворете Package Manager Console:
     ```bash
     Update-Database
     ```
   - Стартирайте приложението с `Ctrl + F5`

5. **Регистрирайте администратор** (или задайте роля в SeedData, ако има такава). Ако потребителя иска може да се регистрира като нормален потребител или да влезе с amin профил. Email: admin@art.sahring ; Password: Admin@12345

## Demo Screenshots
![image](https://github.com/user-attachments/assets/866eb358-bde3-443c-8e00-06683cf70d61)
![image](https://github.com/user-attachments/assets/14876cdf-9ca1-49f9-af95-cdec77ba57c3)
![image](https://github.com/user-attachments/assets/ac52a87f-58ea-4a26-ae0b-47aba1634d01)
![image](https://github.com/user-attachments/assets/89d9f645-b7d7-47a7-90a6-48aab9da4de6)
![image](https://github.com/user-attachments/assets/4e802adf-2c80-4799-9f9d-30cf97dbdce4)
![image](https://github.com/user-attachments/assets/4573d042-3b38-428a-a717-5ce60f9f3303)
![image](https://github.com/user-attachments/assets/eb37763e-2db2-4a76-b26f-d88e8275128c)


## Автор
Приложението е създадено от Калоян Владимиров Савчев по задание за дипломен проект.
## 🪪 Лиценз
Проектът е с отворен код и се разпространява под лиценза **MIT**.
