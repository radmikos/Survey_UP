using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using SurveyUP.Models;
using System.ComponentModel.DataAnnotations;
using SurveyUP.Models.Tables;
using Microsoft.AspNetCore.Identity;

namespace SurveyUP.Pages.Voters
{
    public class VotersModel : PageModel
    {
        private readonly N3mikosContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VotersModel(
            N3mikosContext context,
            UserManager<ApplicationUser> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }

        public class LocalVotes  
        {
            public ApplicationUser Voter;
            [Display(Name = "Tytuł Ankiety")]
            public string SurveyTitle { get; set; }
            [Display(Name = "Głosujący")]
            public int VoterId { get; set; }
            public string UserId { get; set; }
            public int? SurveyId { get; set; }
        }

        public IList<LocalVotes> Votes { get; set; } = new List<LocalVotes>();

        public async Task OnGetAsync(int? id)
        {
            if (id == null)
            {
                return ;
            }

            Votes = await (from v in _context.VtsTbVoter
                            join s in _context.VtsTbSurvey
                            on v.SurveyId equals s.SurveyId into j1
                            from j2 in j1.DefaultIfEmpty()
                            where j2.SurveyId == id
                            select new LocalVotes
                            {
                                SurveyTitle = (from s in _context.VtsTbSurvey
                                               where s.SurveyId == j2.SurveyId
                                               select s.Title).FirstOrDefault(),
                                SurveyId = j2.SurveyId,
                                VoterId = v.VoterId,
                                UserId = v.Uid
                            }).ToListAsync();

            foreach(var v in Votes)
            {
                v.Voter = _userManager.Users.FirstOrDefault(u => u.Id == v.UserId);
            }
        }
    }
}
