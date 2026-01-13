pub trait Analyzer: Send + Sync {
    fn analyze(&self, file_content: &str) -> Vec<String>;
    fn name(&self) -> &str;
}

pub struct TodoAnalyzer;

impl Analyzer for TodoAnalyzer {
    fn analyze(&self, file_content: &str) -> Vec<String> {
        file_content
            .lines()
            .enumerate()
            .filter_map(|(i, line)| {
                if line.contains("// TODO") || line.contains("//TODO") {
                    Some(format!("line {}: found TODO comment", i + 1))
                } else {
                    None
                }
            })
            .collect()
    }

    fn name(&self) -> &str {
        "TODO CHECKER"
    }
}

pub struct LongLineAnalyzer {
    pub max_length: usize,
}

impl Analyzer for LongLineAnalyzer {
    fn analyze(&self, file_content: &str) -> Vec<String> {
        file_content
            .lines()
            .enumerate()
            .filter_map(|(i, line)| {
                let len = line.len();
                if len > self.max_length {
                    Some(format!(
                        "Line {}: too large line ({} chars, max: {})",
                        i + 1,
                        len,
                        self.max_length
                    ))
                } else {
                    None
                }
            })
            .collect()
    }

    fn name(&self) -> &str {
        "Long line checker"
    }
}

pub struct GodClassAnalyzer {
    pub max_lines: usize,
}

impl Analyzer for GodClassAnalyzer {
    fn analyze(&self, file_content: &str) -> Vec<String> {
        let line_count = file_content.lines().count();

        if line_count > self.max_lines {
            vec![format!(
                "God class detected! The file has {} lines (max: {})",
                line_count, self.max_lines
            )]
        } else {
            Vec::new()
        }
    }

    fn name(&self) -> &str {
        "God class checker"
    }
}
