mod analyzer;
mod config;

use std::{fs, path::PathBuf};

use analyzer::{Analyzer, GodClassAnalyzer, LongLineAnalyzer, TodoAnalyzer};
use clap::{arg, command, Parser};
use colored::Colorize;
use config::Config;
use rayon::iter::{IntoParallelRefIterator, ParallelIterator};
use walkdir::WalkDir;

#[derive(Parser, Debug)]
#[command(name = "CodeSentry")]
#[command(about = "stupidly simple static analyzer for c# projects", long_about = None)]
struct Args {
    #[arg(default_value = ".")]
    path: PathBuf,

    #[arg(short, long, default_value = "codesentry.toml")]
    config: PathBuf,
}

#[derive(Debug)]
struct AnalysisResult {
    file_path: String,
    analyzer_name: String,
    issues: Vec<String>,
}

fn main() -> Result<(), anyhow::Error> {
    let args = Args::parse();

    println!("{}", "CodeSentry - Static Analysis Tool".bold().cyan());
    println!("{}", "─".repeat(50));

    let config = if args.config.exists() {
        Config::load(&args.config)?
    } else {
        Config::default()
    };

    let cs_files: Vec<PathBuf> = WalkDir::new(&args.path)
        .into_iter()
        .filter_map(|e| e.ok())
        .filter(|e| e.path().extension().is_some_and(|ext| ext == "cs"))
        .map(|e| e.path().to_path_buf())
        .collect();

    if cs_files.is_empty() {
        return Ok(());
    }

    println!("Found {} .cs files\n", cs_files.len());

    let analyzers = create_analyzers(&config);

    if analyzers.is_empty() {
        println!("{}", "All analyzers are deactivated!".yellow());
        return Ok(());
    }

    println!("active analyzers:");
    for analyzer in &analyzers {
        println!("   • {}", analyzer.name());
    }

    println!();

    let results: Vec<AnalysisResult> = cs_files
        .par_iter()
        .flat_map(|file_path| analyze_file(file_path, &analyzers))
        .collect();

    display_results(&results);

    Ok(())
}

fn create_analyzers(config: &Config) -> Vec<Box<dyn Analyzer>> {
    let mut analyzers: Vec<Box<dyn Analyzer>> = Vec::new();

    if config.analyzers.todo_cheker {
        analyzers.push(Box::new(TodoAnalyzer));
    }

    if config.analyzers.long_line_checker {
        analyzers.push(Box::new(LongLineAnalyzer {
            max_length: config.analyzers.max_line_length,
        }));
    }

    if config.analyzers.god_class_checker {
        analyzers.push(Box::new(GodClassAnalyzer {
            max_lines: config.analyzers.max_class_lines,
        }));
    }

    analyzers
}

fn analyze_file(file_path: &PathBuf, analyzers: &[Box<dyn Analyzer>]) -> Vec<AnalysisResult> {
    let content = match fs::read_to_string(file_path) {
        Ok(c) => c,
        Err(e) => {
            eprintln!("error while reading {:?}: {}", file_path, e);
            return Vec::new();
        }
    };

    let file_path_str = file_path.display().to_string();

    analyzers
        .iter()
        .filter_map(|analyzer| {
            let issues = analyzer.analyze(&content);
            if issues.is_empty() {
                None
            } else {
                Some(AnalysisResult {
                    file_path: file_path_str.clone(),
                    analyzer_name: analyzer.name().to_string(),
                    issues,
                })
            }
        })
        .collect()
}

fn display_results(results: &[AnalysisResult]) {
    if results.is_empty() {
        println!("{}", "No issues found".green().bold());
        return;
    }

    println!("{}", "Results:".bold());
    println!("{}", "─".repeat(50));

    let mut total_issues = 0;

    for result in results {
        println!("\n{} {}", "".bold(), result.file_path.yellow());
        println!("   {} {}", "Analyzer:".bold(), result.analyzer_name.cyan());

        for issue in &result.issues {
            println!("   {} {}", "•".red(), issue);
            total_issues += 1;
        }
    }

    println!("\n{}", "─".repeat(50));
    println!(
        "{} {} issues in {} files",
        "".bold(),
        total_issues.to_string().red().bold(),
        results.len().to_string().yellow()
    );
}
