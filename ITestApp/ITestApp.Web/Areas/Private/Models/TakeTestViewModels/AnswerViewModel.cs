﻿using System.ComponentModel.DataAnnotations;

namespace ITestApp.Web.Areas.Private.Models.TakeTestViewModels
{
    public class AnswerViewModel
    {
        [Required]
        public int Id { get; set; }

        public string Content { get; set; }
    }
}
