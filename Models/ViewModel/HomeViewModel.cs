using System.Collections.Generic;

namespace ELearningPlatform.Models.ViewModel
{
    public class HomeViewModel
    {
        public List<Course> FeaturedCourses { get; set; }
        public List<ReviewViewModel> FeaturedReviews { get; set; }
    }
}

