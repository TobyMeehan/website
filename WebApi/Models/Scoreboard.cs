﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Scoreboard
    {
        public string AppId { get; set; }
        public List<Objective> Objectives { get; set; }
        public List<Score> Scores { get; set; }
    }
}