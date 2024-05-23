using System;
using System.Collections.Generic;

namespace Flipard.MVC.ViewModels
{
    public class HomeCreateSetViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<TermMeaningViewModel> TermMeanings { get; set; } = new List<TermMeaningViewModel>();
    }

    public class TermMeaningViewModel
    {
        public Guid Id { get; set; }
        public string Term { get; set; }
        public string Meaning { get; set; }
        public string? LastReviewed { get; set; }
        public string? ImageUrl { get; set; }
    }
}