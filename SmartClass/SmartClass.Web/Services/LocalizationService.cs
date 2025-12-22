using System;
using System.Collections.Generic;
using Radzen.Blazor; // Хоча цей using не використовується, залишаємо його

namespace SmartClass.Web.Services
{
    public class LocalizationService : ILocalizationService
    {
        // Приклад словника: тут будуть зберігатися всі рядки
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Загальні
            {"Assignment", "Завдання"},
            {"Deadline", "Дедлайн"},
            {"MaxPoints", "Балів"},
            {"Topic", "Тема"},
            {"File", "Файл"},
            {"Size", "Розмір"},
            {"Action", "Дія"},
            {"Edit", "Редагувати"},
            {"Delete", "Видалити"},
            {"Cancel", "Скасувати"},
            {"Yes", "Так"},
            {"No", "Ні"}, // ⬅️ ЗАЛИШАЄМО ТІЛЬКИ ОДИН РАЗ
            {"Comment", "Коментар"},
            {"SubmittedAt", "Надіслано"},
            {"NoDescriptionProvided", "Детальний опис відсутній"},
            {"NoFilesAttached", "Немає прикріплених файлів"},
            {"AssignmentDescription", "Опис завдання"},
            {"AssignmentFiles", "Файли до завдання" },

            // Статуси завдань (Assignment Status)
            {"Draft", "Чернетка"},
            {"Published", "Опубліковано"},
            {"Closed", "Закрито"},
            {"Publish", "Опублікувати"},
            {"Close", "Закрити"},
            {"Status", "Статус"},
            {"Grade", "Оцінка"},

            // Статуси відповідей (Submission Status - Enum)
            {"NotSubmitted", "Не здано"},
            {"Submitted", "Здано"},
            {"Graded", "Оцінено"},
            {"ReturnedForRevision", "На доопрацювання"},
            
            // Викладач (Teacher)
            {"StudentSubmissions", "Відповіді студентів"},
            {"Student", "Студент"},
            // {"SubmittedAt", "Надіслано"}, <-- ВИДАЛЕНО, Оскільки Є У "Загальні"
            {"NoSubmissionsYet", "Поки немає жодної відповіді"},
            {"GradingFor", "Оцінювання ({0})"}, // {0} - StudentName
            {"ScoreMax", "Бал (макс. {0})"},    // {0} - PointsMax
            {"TeacherComment", "Коментар викладача"},
            {"ProvideFeedback", "Надайте зворотний зв'язок"},
            {"SaveGrade", "Зберегти оцінку"},
            {"GradeSaved", "Оцінка збережена"},
            {"ErrorSavingGrade", "Помилка збереження оцінки"},
            {"AssignmentNotFound", "Завдання не знайдено"},
            
            // Студент (Student)
            {"YourWork", "Ваша робота"},
            {"NotGraded", "Не оцінено"},
            {"AttachReplaceFiles", "Прикріпити/замінити файли відповіді"},
            {"SubmitWork", "Здати роботу"},
            {"ResubmitWork", "Перездати роботу"},
            {"AttachedFiles", "Прикріплені файли"},
            {"NoFilesAttachedStudent", "Ви ще не прикріпили жодного файлу."},
            {"WorkSubmitted", "Роботу здано"},
            {"ResponseSentToTeacher", "Ваша відповідь відправлена викладачу."},
            {"ErrorSubmittingWork", "Помилка здачі роботи"},

            // Помилки/Дії
            {"ErrorLoadingSubmission", "Помилка завантаження відповіді"},
            {"ErrorLoadingSubmissions", "Помилка завантаження відповідей"},
            {"ErrorUploadingFile", "Помилка завантаження файлу"},
            {"ResponseUploaded", "Відповідь завантажено"},
            {"FilesAttachedToWork", "Файли прикріплено до вашої роботи."},
            {"ErrorUploadingResponse", "Помилка завантаження відповіді"},
            {"Todo", "TODO"},
            {"StatusChangeNotImplemented", "Зміна статусу поки не реалізована: {0}"},
            {"DeleteAssignment", "Видалення завдання"},
            {"ConfirmDeleteAssignmentTitle", "Видалити це завдання?"},
            {"ConfirmDeleteAssignmentMessage", "Це видалить завдання, усі прикріплені файли та всі відповіді студентів. Дія незворотна."},
            {"DeleteAssignmentConfirmed", "Видалення завдання успішно підтверджено."},
            {"ConfirmFileDeletion", "Ви впевнені, що хочете видалити цей файл?"},
            {"DeleteFile", "Видалити файл"}
        };

        public string Get(string key, params string[] args)
        {
            if (_strings.TryGetValue(key, out var value))
            {
                // Заміна аргументів, якщо вони є
                if (args != null && args.Length > 0)
                {
                    try
                    {
                        return string.Format(value, args);
                    }
                    catch (FormatException)
                    {
                        return $"L!{key} [Format Error]";
                    }
                }
                return value;
            }
            return key; // Повертаємо ключ, якщо переклад не знайдено
        }
    }
}