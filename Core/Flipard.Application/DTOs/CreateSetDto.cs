using Microsoft.AspNetCore.Http;

namespace Flipard.Application.DTOs;

public class CreateSetDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<TermMeaningDto> TermMeanings { get; set; }
    
    public class TermMeaningDto
    {
        public string Term { get; set; }
        public string Meaning { get; set; }
        public IFormFile? Image { get; set; }
    }
}

