using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    public class SiteVM
    {
        public Site NewSite { get; set; } = new Site();
        public IEnumerable<Site> Sites { get; set; } = new List<Site>();
    }
}
