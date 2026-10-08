using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SurveyUP.Models;


namespace SurveyUP.Pages.Voters
{
    public class VoteModel : PageModel
    {
        private readonly N3mikosContext _context;

        [BindProperty]
        public List<QuestionToFill> QuestionsToFill { get; set; }
        public string SurveyName { get; set; }
        public Voter SurveyVoter { get; set; }

        public class VoterAnswer
        {
            public int SectionNumber { get; set; }
            public List<string> Answers { get; set; }
        }

        public class Voter
        {
            public DateTime? VoteDate { get; internal set; }
            public string Ipsource { get; internal set; }
            public DateTime? StartDate { get; internal set; }
            public bool? Validated { get; internal set; }
        }

        public class Answer
        {
            
            public string DefaultText { get; internal set; }
            public short? AnswerTypeId { get; internal set; }
            public int AnswerId { get; internal set; }
            public string AnswerText { get; internal set; }
            public List<VoterAnswer> VoterAnswers { get; set; }
        }

        public class QuestionToFill
        {
            public int QuestionId { get; set; }
            [Display(Name = "Tekst pytania")]
            public string QuestionText { get; set; }
            [Display(Name = "Pozycja pytania")]
            public int? DisplayOrder { get; set; }
            public string QuestionIdText { get; set; }
            public string HelpText { get; set; }
            public string Alias { get; set; }
            
            public List<Answer> Answers { get; set; }
        }

        public VoteModel(N3mikosContext context)
        {
            _context = context;
        }

     

        public IActionResult OnGet(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var voterId = id.Value;
            
            var surveyId = (from v in _context.VtsTbVoter
                            where v.VoterId == voterId
                            select v.SurveyId).FirstOrDefault();

            SurveyName = (from s in _context.VtsTbSurvey
                          where s.SurveyId == surveyId
                          select s.Title).FirstOrDefault();


            SurveyVoter = (from v in _context.VtsTbVoter
                     where v.SurveyId == surveyId && v.VoterId == voterId
                           select new Voter
                     {
                         VoteDate = v.VoteDate,
                         Ipsource = v.Ipsource,
                         StartDate = v.StartDate,
                         Validated = v.Validated,
                     }).FirstOrDefault();

            QuestionsToFill = (from q in _context.VtsTbQuestion
                               where q.SurveyId == surveyId
                               select new QuestionToFill
                               {
                                   QuestionId = q.QuestionId,
                                   QuestionText = q.QuestionText,
                                   DisplayOrder = q.DisplayOrder,
                                   HelpText = q.HelpText,
                                   Alias = q.Alias,
                                   Answers = (from a in _context.VtsTbAnswer
                                                   where q.QuestionId == a.QuestionId
                                                   select new Answer
                                                   {
                                                       DefaultText = a.DefaultText,
                                                       AnswerTypeId = a.AnswerTypeId,
                                                       AnswerId = a.AnswerId,                                                       
                                                       AnswerText = a.AnswerText,
                                                       VoterAnswers = (from va in _context.VtsTbVoterAnswers
                                                                       where va.VoterId == voterId && va.AnswerId == a.AnswerId
                                                                       select new VoterAnswer
                                                                       {
                                                                           Answers = (va.AnswerText ?? string.Empty).Split('|', StringSplitOptions.None).ToList(),
                                                                           SectionNumber = va.SectionNumber
                                                                       }).ToList()
                                                   }).ToList()
                               }).ToList();




            QuestionsToFill = QuestionsToFill.OrderBy(q => q.DisplayOrder).ToList();


            return Page();
        }



        
    }
}
