﻿using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ErrorIgnoreAttribute : Attribute
{
}