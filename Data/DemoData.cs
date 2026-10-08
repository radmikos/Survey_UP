using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SurveyUP.Models;
using SurveyUP.Models.Tables;
using SurveyUP.Tables;

namespace SurveyUP.Data
{
    /// <summary>Fictional sample data for local development, demos and screenshots.</summary>
    public static class DemoData
    {
        public static async Task SeedAsync(N3mikosContext db, UserManager<ApplicationUser> users)
        {
            if (!await db.VtsTbAnswerType.AnyAsync())
            {
                db.VtsTbAnswerType.AddRange(
                    new VtsTbAnswerType { AnswerTypeId = 1, Description = "Jednokrotny wybór w linii" },
                    new VtsTbAnswerType { AnswerTypeId = 2, Description = "Wielokrotny wybór w linii" },
                    new VtsTbAnswerType { AnswerTypeId = 3, Description = "Pole tekstowe" },
                    new VtsTbAnswerType { AnswerTypeId = 4, Description = "Jednokrotny wybór" },
                    new VtsTbAnswerType { AnswerTypeId = 20, Description = "Wielokrotny wybór" },
                    new VtsTbAnswerType { AnswerTypeId = 21, Description = "Data i godzina" });
                await db.SaveChangesAsync();
            }

            if (await db.VtsTbSurvey.AnyAsync()) return;

            var survey = new VtsTbSurvey
            {
                Title = "Ankieta oceny praktyk zawodowych 2026",
                ThankYouMessage = "Dziękujemy za wypełnienie ankiety. Twoje odpowiedzi pomogą nam ulepszyć program praktyk.",
                OpenDate = DateTime.Today.AddDays(-14),
                CloseDate = DateTime.Today.AddDays(30),
                Activated = true,
                Scored = false,
                SurveyGuid = Guid.NewGuid()
            };
            var archived = new VtsTbSurvey
            {
                Title = "Ankieta satysfakcji z zajęć (semestr zimowy 2025)",
                ThankYouMessage = "Dziękujemy!",
                OpenDate = DateTime.Today.AddMonths(-9),
                CloseDate = DateTime.Today.AddMonths(-8),
                Archive = true,
                Scored = false,
                SurveyGuid = Guid.NewGuid()
            };
            db.VtsTbSurvey.AddRange(survey, archived);
            await db.SaveChangesAsync();

            VtsTbQuestion Q(string text, int order, string help = null) =>
                new VtsTbQuestion { SurveyId = survey.SurveyId, QuestionText = text, DisplayOrder = order, PageNumber = 1, ColumnsNumber = 1, HelpText = help, ShowHelpText = help != null };

            var q1 = Q("Jak oceniasz organizację praktyk w miejscu pracy?", 1, "Wybierz jedną odpowiedź.");
            var q2 = Q("Jakie umiejętności rozwinąłeś podczas praktyk?", 2, "Możesz zaznaczyć kilka odpowiedzi.");
            var q3 = Q("Czy poleciłbyś to miejsce praktyk innym studentom?", 3);
            var q4 = Q("Co należałoby poprawić w programie praktyk?", 4);
            db.VtsTbQuestion.AddRange(q1, q2, q3, q4);
            await db.SaveChangesAsync();

            var answers = new List<VtsTbAnswer>();
            VtsTbAnswer A(VtsTbQuestion q, short type, string text, int order) =>
                new VtsTbAnswer { QuestionId = q.QuestionId, AnswerTypeId = type, AnswerText = text, DisplayOrder = order };
            string[] scale = { "Bardzo dobrze", "Dobrze", "Przeciętnie", "Słabo" };
            for (int i = 0; i < scale.Length; i++) answers.Add(A(q1, 4, scale[i], i + 1));
            string[] skills = { "Praca zespołowa", "Programowanie", "Komunikacja z klientem", "Zarządzanie czasem", "Analiza danych" };
            for (int i = 0; i < skills.Length; i++) answers.Add(A(q2, 20, skills[i], i + 1));
            answers.Add(A(q3, 1, "Tak", 1));
            answers.Add(A(q3, 1, "Raczej tak", 2));
            answers.Add(A(q3, 1, "Raczej nie", 3));
            answers.Add(A(q3, 1, "Nie", 4));
            answers.Add(A(q4, 3, "Twoja uwaga", 1));
            db.VtsTbAnswer.AddRange(answers);
            await db.SaveChangesAsync();

            var student = await users.FindByEmailAsync("student@demo.local");
            if (student != null)
            {
                db.VtsTbUserSurvey.Add(new VtsTbUserSurvey { UserId = student.Id, SurveyId = survey.SurveyId });
            }

            // a handful of completed responses so the results screens are not empty
            var rnd = new Random(7);
            var q1a = answers.Where(a => a.QuestionId == q1.QuestionId).ToList();
            var q2a = answers.Where(a => a.QuestionId == q2.QuestionId).ToList();
            var q3a = answers.Where(a => a.QuestionId == q3.QuestionId).ToList();
            var q4a = answers.First(a => a.QuestionId == q4.QuestionId);
            string[] remarks = { "Więcej czasu na zadania praktyczne.", "Lepsza komunikacja z opiekunem praktyk.", "Brak uwag.", "Dłuższy okres praktyk." };
            for (int i = 0; i < 12; i++)
            {
                var voter = new VtsTbVoter
                {
                    Uid = Guid.NewGuid().ToString(),
                    SurveyId = survey.SurveyId,
                    ContextUserName = $"student{i + 1}@demo.local",
                    StartDate = DateTime.Now.AddDays(-rnd.Next(1, 10)),
                    VoteDate = DateTime.Now.AddDays(-rnd.Next(0, 9)),
                    Ipsource = "127.0.0.1",
                    Validated = true
                };
                db.VtsTbVoter.Add(voter);
                await db.SaveChangesAsync();
                var chosen = new List<VtsTbAnswer> { q1a[rnd.Next(q1a.Count - 1)], q3a[rnd.Next(q3a.Count - 1)] };
                chosen.AddRange(q2a.OrderBy(_ => rnd.Next()).Take(rnd.Next(1, 4)));
                foreach (var a in chosen)
                    db.VtsTbVoterAnswers.Add(new VtsTbVoterAnswers { VoterId = voter.VoterId, AnswerId = a.AnswerId, SectionNumber = 1, AnswerText = "checked" });
                db.VtsTbVoterAnswers.Add(new VtsTbVoterAnswers { VoterId = voter.VoterId, AnswerId = q4a.AnswerId, SectionNumber = 1, AnswerText = remarks[rnd.Next(remarks.Length)] });
                await db.SaveChangesAsync();
            }
            await db.SaveChangesAsync();
        }
    }
}
