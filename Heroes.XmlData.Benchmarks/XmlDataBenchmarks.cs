using BenchmarkDotNet.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heroes.XmlData.Benchmarks;

[MemoryDiagnoser]
public class XmlDataBenchmarks
{
    public struct Struct1
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }

        public Class1 Class1 { get; set; }
    }
    public record Record1
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }

        public Class1 Class1 { get; set; }
    }

    public class Class1
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }

        public Class1 ClassOfItself { get; set; }
    }


    //private readonly string _value = "thisIsAstringValueOfSoMeSorts";

    [Benchmark]
    public Struct1 Struct()
    {
        Struct1 struct1 = new Struct1()
        {
            Value1 = "item",
            Value2 = "item2",
            Class1 = new Class1()
            {
                Value1 = "item4",
                Value2 = "item5",
            },
        };

        Dictionary<string, Struct1> dic = [];
        dic.Add("item1", struct1);

        return dic["item1"];
    }

    [Benchmark]
    public Record1 Record()
    {
        Record1 struct1 = new Record1()
        {
            Value1 = "item",
            Value2 = "item2",
            Class1 = new Class1()
            {
                Value1 = "item4",
                Value2 = "item5",
            },
        };

        Dictionary<string, Record1> dic = [];
        dic.Add("item1", struct1);

        return dic["item1"];
    }

    [Benchmark]
    public Class1 Class()
    {
        Class1 struct1 = new Class1()
        {
            Value1 = "item",
            Value2 = "item2",
            ClassOfItself = new Class1()
            {
                Value1 = "item4",
                Value2 = "item5",
            },
        };

        Dictionary<string, Class1> dic = [];
        dic.Add("item1", struct1);

        return dic["item1"];
    }

    //[Benchmark]
    //public string TestUpper()
    //{
    //    string value = "thisIsAstringValueOfSoMeSorts";
    //    string toUpper = value;

    //    if (toUpper == "THISISASTRINGVALUEOFSOMESORTS")
    //    {
    //        return value;
    //    }
    //    return string.Empty;
    //}

    //[Benchmark]
    //public string TestAsSpan()
    //{
    //    string value = "thisIsAstringValueOfSoMeSorts";
    //    if (value.AsSpan().Equals("THISISASTRINGVALUEOFSOMESORTS", StringComparison.OrdinalIgnoreCase))
    //    {
    //        return value;
    //    }
    //    return string.Empty;
    //}

    //[Benchmark]
    //public string TestCompareNormal()
    //{
    //    string value = "thisIsAstringValueOfSoMeSorts";
    //    if (value.Equals("thisIsAstringValueOfSoMeSorts", StringComparison.OrdinalIgnoreCase))
    //    {
    //        return value;
    //    }
    //    return string.Empty;
    //}
}
