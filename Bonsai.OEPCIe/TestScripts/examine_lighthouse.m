
%% Which sensor?
idx = '3';

%% Load raw data
% fid = fopen(['timestamp' idx '.csv']);
% x = textscan(fid,'%d -%d -%d T%d :%d :%f -%s');
% time_sec_0 =  double(x{4}(1) * 3600 + x{5}(1) * 60) + x{6}(1);
% ts = double(x{4}(:) .* 3600) + double(x{5}(:) .* 60) + x{6}(:) - time_sec_0;
% fclose(fid);

fid = fopen('lh1.raw');
type = fread(fid, Inf, 'int16');
fclose(fid);



%% Cut off first few samples (seems like they are stuck in a pipe...)
% n_cut = 30;
% ts = ts(n_cut:end);
% measurement = measurement(n_cut:end);
% type = type(n_cut:end);
% remote_time = remote_time(n_cut:end);

%% Plots
close all
plot(remote_time,'-o')

%%
close all
stairs(remote_time(1:end/2), measurement(1:end/2))
ylim([-0.1 1.1])

%%
histogram(diff(remote_time(measurement == 1)),[-0.001:0.00001:0.006])
% plot(diff(remote_time))
% ylim([-0.001 0.006])

%% Find first rising edge

t1 = remote_time(measurement == 1);
t0 = remote_time(measurement == 0);
b = remote_time > t1(1) & remote_time < t0(end);

t = remote_time(b);
m = measurement(b);


t1 = t(m == 1);
t0 = t(m == 0);

t1 - t0

