using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SurveyUP.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SurveyUP.Pages.Voters
{
    public class DetailsModel : PageModel
    {
        private readonly N3mikosContext _context;

        public DetailsModel(N3mikosContext context)
        {
            _context = context;
        }

        public class AllAnswer
        {
            [Display(Name = "ID Głosowania")]
            public int VoterId { get; set; }
            public int AnswerId { get; set; }
            [Display(Name = "Odpowiedź")]
            public string AnswerText { get; set; }
            public int SectionNumber { get; set; }
            [Display(Name = "Wartość")]
            public List<string> Answers { get; set; }
        }

        public List<AllAnswer> AllAnswers { get; set; }

        public string GetAnswer(string answer)
        {
            var returnString = answer;

            if (Int32.TryParse(answer, out int answerId))
            {
                returnString = (from a in _context.VtsTbAnswer
                                where a.AnswerId == answerId
                                select a.AnswerText).FirstOrDefault();
            }
            return returnString;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AllAnswers = (from va in _context.VtsTbVoterAnswers
                          join v in _context.VtsTbVoter
                          on va.VoterId equals v.VoterId
                          where va.AnswerId == id
                          select new AllAnswer
                          {
                              VoterId = va.VoterId,
                              AnswerId = va.AnswerId,
                              AnswerText = (from a in _context.VtsTbAnswer where a.AnswerId == va.AnswerId select a.AnswerText).FirstOrDefault(),
                              SectionNumber = va.SectionNumber,
                              Answers = (va.AnswerText ?? string.Empty).Split('|', StringSplitOptions.None).ToList()
                          }).ToList();


            return Page();
        }
    }
}
