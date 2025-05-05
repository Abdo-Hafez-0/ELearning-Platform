using System.Collections.Generic;
using System.Linq;

namespace ELearningPlatform.Models.ViewModel
{
    public class MyLearningViewModel
    {
        public List<MyLearning> Items { get; set; } = new List<MyLearning>();
        
        public decimal Subtotal => Items.Sum(item => item.Course.Price * item.Quantity);
        
        public decimal Tax => Subtotal * 0.1m; // 10% tax
        
        public decimal Total => Subtotal + Tax;
        
        public int ItemCount => Items.Sum(item => item.Quantity);
    }
}

