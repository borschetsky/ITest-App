﻿using ITestApp.Common.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITestApp.Web.Models.TakeTestViewModels
{
    public class QuestionViewModel
    {
        public string Id { get; set; }

        [Required]
        [MinLength(ModelConstants.MinQuestionContentLength)]
        [MaxLength(ModelConstants.MaxQuestionContentLength)]
        [DataType(DataType.Text)]
        public string Content { get; set; }

        public IList<AnswerViewModel> Answers { get; set; }

        public string AndswerId { get; set; }
    }
}
