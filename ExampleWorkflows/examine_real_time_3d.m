% Read back the real-time processed lh data to check the operation of
% bonsai processing

%% Sensor positions
fid = fopen('lh1-position.csv');
x = textscan(fid,'%f', 'Delimiter',',');
x = reshape(x{1}, 4, []);
t1 = x(1, :)';
p1 = x(2:4, :)';

fid = fopen('lh2-position.csv');
x = textscan(fid,'%f', 'Delimiter',',');
x = reshape(x{1}, 4, []);
t2 = x(1, :)';
p2 = x(2:4, :)';

% fid = fopen('lh3-position.csv');
% x = textscan(fid,'%f', 'Delimiter',',');
% x = reshape(x{1}, 4, []);
% t3 = x(1, :)';
% p3 = x(2:4, :)';

fid = fopen('lh4-position.csv');
x = textscan(fid,'%f', 'Delimiter',',');
x = reshape(x{1}, 4, []);
t4 = x(1, :)';
p4 = x(2:4, :)';


close all
figure
hold on
plot(t1, p1,'.')
plot(t2, p2,'.')
plot(t4, p4,'.')

%% Look at orientation
figure 
fid = fopen('orientation-result.raw');
q = fread(fid, [100000,1], 'double');
q = reshape(q, 4, []);
plot(q','-')
