﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITestApp.Web.Models.TestViewModels
{
    public class PostQuestionViewModel
    {
        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        [DataType(DataType.Text)]
        public string Content { get; set; }

        public ICollection<PostAnswerViewModel> Answers { get; set; }
    }
}
